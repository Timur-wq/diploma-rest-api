using Diploma.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using static Diploma.Core.Class1;
using Microsoft.Extensions.Logging;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Npgsql;

namespace Diploma.Application.Services
{
    public class ProcurementOptimizationService : IProcurementOptimizationService
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<ProcurementOptimizationService> _logger;

        public ProcurementOptimizationService(
            DiplomaDbContext context,
            ILogger<ProcurementOptimizationService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<ProcurementResultDto> OptimizeProcurementAsync(int actCode)
        {
            _logger.LogInformation("🔍 Запуск жадного алгоритма для акта #{ActCode}", actCode);

            var result = new ProcurementResultDto { ActCode = actCode };

            try
            {
                // 1. Получаем одобренные ЗИП из акта
                var approvedSpares = await _context.SparePartsDiagnosticActs
                    .Include(s => s.Spare)
                    .Where(s => s.ActCode == actCode &&
                               (s.IsApprovedByClient == true || s.IsApprovedByClient == null))
                    .ToListAsync();

                _logger.LogInformation("📦 Найдено одобренных ЗИП: {Count}", approvedSpares.Count);

                if (!approvedSpares.Any())
                {
                    result.Message = "Нет одобренных ЗИП для закупки";
                    result.IsFullySatisfied = true;
                    return result;
                }

                // 2. Формируем потребность
                var demand = approvedSpares
                    .GroupBy(s => s.SpareCode)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(s => s.Amount)
                    );

                _logger.LogInformation("📋 Потребность: {@Demand}", demand);

                // 🔥 3. ИСПРАВЛЕНИЕ: Используем .Contains() вместо Dictionary.ContainsKey()
                var spareCodes = demand.Keys.ToList();

                var supplierOffers = await _context.SupplierSpares
                    .Include(ss => ss.Spare)
                    .Include(ss => ss.Supplier)
                    .Where(ss => spareCodes.Contains(ss.SpareCode) &&
                                ss.QuantityInStock > 0)
                    .ToListAsync();

                _logger.LogInformation("🏪 Найдено предложений: {Count}", supplierOffers.Count);

                if (!supplierOffers.Any())
                {
                    result.Message = "Нет доступных предложений от поставщиков";
                    result.IsFullySatisfied = false;
                    return result;
                }

                // 4. Запускаем жадный алгоритм
                var allocation = await RunGreedyAlgorithmAsync(demand, supplierOffers);

                // 🔥 Логирование результатов жадного алгоритма перед сохранением
                _logger.LogInformation("📊 Результаты распределения закупок:");
                foreach (var ((spareCode, supplierId), quantity) in allocation)
                {
                    var offer = supplierOffers.FirstOrDefault(o => o.SpareCode == spareCode && o.SupplierId == supplierId);
                    if (offer != null)
                    {
                        _logger.LogInformation("  📦 ЗИП #{SpareCode} ({SpareName}) | Поставщик #{SupplierId} ({SupplierName}) | {Quantity} шт. × {UnitPrice} ₽ = {Total} ₽",
                            spareCode,
                            offer.Spare?.SpareName ?? "Неизвестно",
                            supplierId,
                            offer.Supplier?.Name ?? "Неизвестно",
                            quantity,
                            offer.UnitPrice,
                            quantity * offer.UnitPrice);
                    }
                }

                // 🔥 Получаем последний код заявки на ЗИП
                var lastOrderCode = await _context.SpareOrders
                    .MaxAsync(o => (int?)o.OrderId) ?? 0;

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var ((spareCode, supplierId), quantity) in allocation)
                    {
                        // 🔥 НАХОДИМ предложение поставщика
                        var offer = supplierOffers.FirstOrDefault(o =>
                            o.SpareCode == spareCode && o.SupplierId == supplierId);

                        if (offer == null)
                        {
                            _logger.LogWarning("⚠️ Не найдено предложение для ЗИП #{SpareCode} от поставщика #{SupplierId}",
                                spareCode, supplierId);
                            continue;
                        }

                        // 🔥 ИСПОЛЬЗУЕМ offer.Id (это SupplierSpareId), а не supplierId!
                        var existingOrder = await _context.SpareOrders
                            .FirstOrDefaultAsync(o =>
                                o.SupplierSpareId == offer.Id &&  // ✅ ИСПРАВЛЕНО: offer.Id вместо supplierId
                                o.Status == SpareOrderStatus.Reservation &&
                                o.ActCode == actCode);

                        if (existingOrder == null)
                        {
                            var order = new SpareOrderEntity
                            {
                                SupplierSpareId = offer.Id,  // ✅ ИСПРАВЛЕНО: offer.Id вместо supplierId
                                Quantity = quantity,
                                Status = SpareOrderStatus.Confirmed,
                                CreatedAt = DateTime.UtcNow,
                                ActCode = actCode
                            };
                            _context.SpareOrders.Add(order);

                            _logger.LogInformation("➕ Создана новая заявка: OrderId={OrderId}, SupplierSpareId={SupplierSpareId}, ЗИП #{SpareCode}, Поставщик #{SupplierId}, {Quantity} шт.",
                                order.OrderId, offer.Id, spareCode, supplierId, quantity);
                        }
                        else
                        {
                            existingOrder.Quantity += quantity;
                            _logger.LogInformation("✏️ Обновлена заявка #{OrderId}: SupplierSpareId={SupplierSpareId}, было {OldQty} шт., стало {NewQty} шт.",
                                existingOrder.OrderId, offer.Id, existingOrder.Quantity - quantity, existingOrder.Quantity);
                        }
                    }

                    // 🔥 ОДИН SaveChanges для ВСЕХ изменений — атомарно + быстро
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 🔥 Итоговое логирование после успешного сохранения
                    _logger.LogInformation("💾 Все заявки успешно сохранены в БД");
                }
                catch (DbUpdateException ex) when
                    (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("⚠️ Конфликт уникальности (23505) — возможно, параллельный запрос. Повторите операцию.");
                    result.Message = "⚠️ Часть заявок не создана из-за параллельного запроса";
                    return result;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }

                // 6. Формируем результат
                result = BuildResultDto(actCode, demand, allocation, approvedSpares);

                // 🔥 Детальное логирование итогового результата
                _logger.LogInformation("📋 ИТОГОВЫЙ ОТЧЁТ ПО ЗАКУПКЕ (Акт #{ActCode}):", actCode);
                _logger.LogInformation("  💰 Общая стоимость: {TotalCost} ₽", result.TotalCost);
                _logger.LogInformation("  ✅ Полностью удовлетворено: {IsSatisfied}", result.IsFullySatisfied ? "Да" : "Нет");

                foreach (var item in result.ProcurementItems)
                {
                    _logger.LogInformation("  📦 ЗИП #{SpareCode} ({SpareName}): требуется {Required} шт., закуплено {Procured} шт.",
                        item.SpareCode, item.SpareName, item.RequiredQuantity, item.ProcuredQuantity);

                    foreach (var alloc in item.SupplierAllocations)
                    {
                        _logger.LogInformation("    ├─ Поставщик #{SupplierId} ({SupplierName}): {Qty} шт. × {Price} ₽ = {Sum} ₽",
                            alloc.SupplierId, alloc.SupplierName, alloc.Quantity, alloc.UnitPrice, alloc.Quantity * alloc.UnitPrice);
                    }
                }

                if (result.SupplierSummaries.Any())
                {
                    _logger.LogInformation("  🏪 Сводка по поставщикам:");
                    foreach (var summary in result.SupplierSummaries)
                    {
                        _logger.LogInformation("    ├─ #{SupplierId} ({SupplierName}): {Items} позиций, {Qty} шт., на сумму {Cost} ₽",
                            summary.SupplierId, summary.SupplierName, summary.TotalItems, summary.TotalQuantity, summary.TotalCost);
                    }
                }

                if (result.UnmetDemands.Any())
                {
                    _logger.LogWarning("  ⚠️ Неудовлетворённые потребности:");
                    foreach (var unmet in result.UnmetDemands)
                    {
                        _logger.LogWarning("    ├─ ЗИП #{SpareCode} ({SpareName}): требуется {Required}, закуплено {Procured}, дефицит {Shortage} шт.",
                            unmet.SpareCode, unmet.SpareName, unmet.Required, unmet.Procured, unmet.Shortage);
                    }
                }

                _logger.LogInformation("✅ Алгоритм завершён. Стоимость: {TotalCost} ₽, Заявок создано/обновлено: {Orders}",
                    result.TotalCost, allocation.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка в алгоритме оптимизации для акта #{ActCode}", actCode);
                throw;
            }
        }

        /// <summary>
        /// 🔥 Жадный алгоритм назначения поставщиков
        /// </summary>
        private async Task<Dictionary<(int spareCode, int supplierId), int>> RunGreedyAlgorithmAsync(
            Dictionary<int, int> demand,
            List<SupplierSpareEntity> supplierOffers)
        {
            var allocation = new Dictionary<(int, int), int>();  // (SpareCode, SupplierId) -> Quantity

            // Копия потребности для модификации
            var remainingDemand = new Dictionary<int, int>(demand);

            // Копия остатков поставщиков для модификации
            var remainingStock = supplierOffers
                .ToDictionary(
                    ss => (ss.SpareCode, ss.SupplierId),
                    ss => ss.QuantityInStock
                );

            // Группируем предложения по ЗИП для быстрого доступа
            var offersBySpare = supplierOffers
                .GroupBy(ss => ss.SpareCode)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 🔥 Основной цикл жадного алгоритма
            while (remainingDemand.Any(kvp => kvp.Value > 0))
            {
                // Шаг 2: Выбираем ЗИП с наибольшей неудовлетворённой потребностью
                var maxDemandItem = remainingDemand
                    .Where(kvp => kvp.Value > 0)
                    .OrderByDescending(kvp => kvp.Value)
                    .First();

                int spareCode = maxDemandItem.Key;
                int remainingNeed = maxDemandItem.Value;

                // Шаг 3: Находим поставщика с наименьшей ценой, у которого есть остаток
                var availableOffers = offersBySpare.TryGetValue(spareCode, out var offers)
                    ? offers.Where(o => remainingStock.TryGetValue((o.SpareCode, o.SupplierId), out var stock) && stock > 0)
                            .OrderBy(o => o.UnitPrice)
                    : Enumerable.Empty<SupplierSpareEntity>();

                foreach (var offer in availableOffers)
                {
                    if (remainingNeed <= 0) break;

                    var key = (offer.SpareCode, offer.SupplierId);

                    if (!remainingStock.TryGetValue(key, out var availableStock) || availableStock <= 0)
                        continue;

                    // Шаг 4: Определяем объем закупки
                    int volume = Math.Min(remainingNeed, availableStock);

                    // Шаг 5: Фиксируем закупку
                    if (allocation.TryGetValue(key, out var existingQty))
                        allocation[key] = existingQty + volume;
                    else
                        allocation[key] = volume;

                    // Шаг 6: Обновляем остатки
                    remainingStock[key] -= volume;
                    remainingDemand[spareCode] -= volume;
                    remainingNeed -= volume;

                    _logger.LogDebug("  ➕ Закупка: ЗИП #{SpareCode} у поставщика #{SupplierId}: {Volume} шт. по {Price} ₽",
                        spareCode, offer.Id, volume, offer.UnitPrice);
                }

                // Если не удалось полностью удовлетворить потребность — переходим к следующему ЗИП
                // (алгоритм продолжит работу с другими ЗИП, потом вернётся если возможно)
            }

            return allocation;
        }

        /// <summary>
        /// Формирует DTO результата из данных алгоритма
        /// </summary>
        private ProcurementResultDto BuildResultDto(
            int actCode,
            Dictionary<int, int> demand,
            Dictionary<(int spareCode, int supplierId), int> allocation,
            List<SparePartsDiagnosticAct> approvedSpares)
        {
            var result = new ProcurementResultDto
            {
                ActCode = actCode,
                CalculatedAt = DateTime.UtcNow
            };

            var spareLookup = approvedSpares.ToDictionary(s => s.SpareCode, s => s.Spare);

            // Формируем элементы матрицы закупок
            foreach (var (spareCode, requiredQty) in demand)
            {
                var spareName = spareLookup.TryGetValue(spareCode, out var spare)
                    ? spare.SpareName
                    : $"ЗИП #{spareCode}";

                var item = new ProcurementItemDto
                {
                    SpareCode = spareCode,
                    SpareName = spareName,
                    RequiredQuantity = requiredQty,
                    ProcuredQuantity = 0
                };

                // Собираем закупки по поставщикам для этого ЗИП
                foreach (var ((sCode, supplierId), quantity) in allocation)
                {
                    if (sCode != spareCode) continue;

                    var offer = _context.SupplierSpares
                        .Include(ss => ss.Supplier)
                        .FirstOrDefault(ss => ss.SpareCode == spareCode && ss.SupplierId == supplierId);

                    if (offer == null) continue;

                    item.SupplierAllocations.Add(new SupplierProcurementDto
                    {
                        SupplierId = supplierId,
                        SupplierName = offer.Supplier?.Name ?? $"Поставщик #{supplierId}",
                        Quantity = quantity,
                        UnitPrice = offer.UnitPrice
                    });

                    item.ProcuredQuantity += quantity;
                }

                result.ProcurementItems.Add(item);
            }

            // Формируем сводку по поставщикам
            var supplierGroups = allocation
                .GroupBy(kvp => kvp.Key.supplierId)
                .Select(g => new
                {
                    SupplierId = g.Key,
                    Items = g.Count(),
                    Quantity = g.Sum(kvp => kvp.Value),
                    Cost = g.Sum(kvp =>
                    {
                        var offer = _context.SupplierSpares
                            .FirstOrDefault(ss => ss.SpareCode == kvp.Key.spareCode && ss.SupplierId == kvp.Key.supplierId);
                        return offer?.UnitPrice * kvp.Value ?? 0;
                    })
                });

            foreach (var sg in supplierGroups)
            {
                var supplier = _context.SupplierEntities.Find(sg.SupplierId);

                result.SupplierSummaries.Add(new SupplierSummaryDto
                {
                    SupplierId = sg.SupplierId,
                    SupplierName = supplier?.Name ?? $"Поставщик #{sg.SupplierId}",
                    TotalItems = sg.Items,
                    TotalQuantity = sg.Quantity,
                    TotalCost = sg.Cost
                });

                result.TotalCost += sg.Cost;
            }

            // Проверяем полноту удовлетворения
            var totalRequired = demand.Values.Sum();
            var totalProcured = result.ProcurementItems.Sum(i => i.ProcuredQuantity);
            result.IsFullySatisfied = totalProcured >= totalRequired;

            // Формируем список невыполненных потребностей
            result.UnmetDemands = result.ProcurementItems
                .Where(i => i.ProcuredQuantity < i.RequiredQuantity)
                .Select(i => new UnmetDemandDto
                {
                    SpareCode = i.SpareCode,
                    SpareName = i.SpareName,
                    Required = i.RequiredQuantity,
                    Procured = i.ProcuredQuantity
                }).ToList();

            // Формируем сообщение
            if (result.IsFullySatisfied)
            {
                result.Message = $"✅ Все потребности удовлетворены. Общая стоимость: {result.TotalCost:F2} ₽";
            }
            else if (result.UnmetDemands.Any())
            {
                var shortage = result.UnmetDemands.Sum(u => u.Shortage);
                result.Message = $"⚠️ Не удалось полностью удовлетворить потребность: не хватает {shortage} шт. ЗИП";
            }
            else
            {
                result.Message = "✅ Закупка сформирована";
            }

            return result;
        }
    }
}

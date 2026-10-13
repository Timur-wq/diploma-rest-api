// Diploma.Application/Services/DiagnosticActService.cs

using Diploma.Core.Abstractions;
using Diploma.Core.Enums;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Diploma.Core.Class1;

namespace Diploma.Application.Services
{
    public class DiagnosticActService : IDiagnosticActService
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<DiagnosticActService> _logger;

        public DiagnosticActService(DiplomaDbContext context, ILogger<DiagnosticActService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // === Создание акта (инженер) ===
        // Diploma.Application/Services/DiagnosticActService.cs

        public async Task<DiagnosticActResponseDto> CreateActAsync(CreateDiagnosticActRequest request, int engineerTabNumber)
        {
            _logger.LogInformation("📝 Создание акта для наряда #{TaskId}", request.TaskId);

            try
            {
                // 1. Проверка наряда
                var task = await _context.TaskEntities
                    .Include(t => t.StafEntity)
                    .Include(t => t.RequestEntity)
                    .FirstOrDefaultAsync(t => t.WorkOrderId == request.TaskId);

                if (task == null)
                    return new DiagnosticActResponseDto { Success = false, Message = "Наряд не найден" };

                if (task.ServiceEngineerTabNumber != engineerTabNumber)
                    return new DiagnosticActResponseDto { Success = false, Message = "Наряд не ваш" };

                // 2. Проверка, что акт ещё не создан
                var existing = await _context.DiagnosticActs.FirstOrDefaultAsync(a => a.TaskId == request.TaskId);
                if (existing != null)
                    return new DiagnosticActResponseDto { Success = false, Message = "Акт уже создан" };

                // 3. Парсинг даты
                if (!DateOnly.TryParse(request.DiagnosticDate, out var diagnosticDate))
                    diagnosticDate = DateOnly.FromDateTime(DateTime.UtcNow);

                // 4. Создаём акт
                var act = new DiagnosticAct
                {
                    TaskId = request.TaskId,
                    DateOnly = diagnosticDate,
                    ExternalCondition = request.ExternalCondition,
                    IdentifiedIssues = request.IdentifiedIssues,
                    TestResults = request.TestResults,
                    Recommendations = request.Recommendations,
                    EstimatedCost = request.EstimatedCost,
                    EstimatedTime = request.EstimatedTime,
                    Status = DiagnosticActStatus.Submitted
                };

                _context.DiagnosticActs.Add(act);

                // 🔥 ВАЖНО: Сохраняем акт, чтобы получить ActCode для связей
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Акт создан: ActCode={ActCode}", act.ActCode);

                // 5. Сохраняем работы с привязанными ЗИП
                if (request.RequiredWorks != null && request.RequiredWorks.Any())
                {
                    _logger.LogInformation("🔧 Сохранение работ ({Count} позиций)", request.RequiredWorks.Count);

                    foreach (var work in request.RequiredWorks)
                    {
                        // 5.1. Добавляем связь работа-акт
                        var actWork = new WorksDiagnosticAct
                        {
                            ActCode = act.ActCode,
                            WorkCode = work.WorkCode
                        };
                        _context.WorkDiagnosticActs.Add(actWork);
                        _logger.LogDebug("  ➕ Работа #{WorkCode} привязана к акту", work.WorkCode);

                        // 5.2. 🔥 Добавляем ЗИП и связи работа-ЗИП
                        if (work.RequiredSpares != null && work.RequiredSpares.Any())
                        {
                            foreach (var spareLink in work.RequiredSpares)
                            {
                                // Проверяем, не добавлен ли уже этот ЗИП в акт
                                var existingSpare = await _context.SparePartsDiagnosticActs
                                    .FirstOrDefaultAsync(s => s.ActCode == act.ActCode && s.SpareCode == spareLink.SpareCode);

                                if (existingSpare == null)
                                {
                                    // Добавляем ЗИП в акт
                                    var actSpare = new SparePartsDiagnosticAct
                                    {
                                        ActCode = act.ActCode,
                                        SpareCode = spareLink.SpareCode,
                                        Amount = spareLink.Quantity
                                    };
                                    _context.SparePartsDiagnosticActs.Add(actSpare);

                                    // 🔥 ДОБАВЛЯЕМ СВЯЗЬ работа-ЗИП в таблицу связей
                                    var relation = new WorkSpareRelationInAct
                                    {
                                        ActCode = act.ActCode,
                                        WorkCode = work.WorkCode,
                                        SpareCode = spareLink.SpareCode,
                                        Quantity = spareLink.Quantity,
                                        IsRequired = spareLink.IsRequired
                                    };
                                    _context.WorkSpareRelationsInAct.Add(relation);

                                    _logger.LogDebug("    ➕ ЗИП #{SpareCode} (кол-во: {Qty}) привязан к работе #{WorkCode}",
                                        spareLink.SpareCode, spareLink.Quantity, work.WorkCode);
                                }
                            }
                        }
                    }
                }

                // 6. Сохраняем общие ЗИП (не привязанные к работам)
                if (request.RequiredSpares != null && request.RequiredSpares.Any())
                {
                    _logger.LogInformation("📦 Сохранение общих ЗИП ({Count} позиций)", request.RequiredSpares.Count);

                    foreach (var spare in request.RequiredSpares)
                    {
                        // Проверяем, не добавлен ли уже этот ЗИП через работы
                        var exists = await _context.SparePartsDiagnosticActs
                            .AnyAsync(s => s.ActCode == act.ActCode && s.SpareCode == spare.SpareCode);

                        if (!exists)
                        {
                            _context.SparePartsDiagnosticActs.Add(new SparePartsDiagnosticAct
                            {
                                ActCode = act.ActCode,
                                SpareCode = spare.SpareCode,
                                Amount = spare.Quantity
                            });
                        }
                    }
                }

                // 🔥 ОДИН вызов SaveChanges для всех связей
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Работы, ЗИП и связи сохранены");

                // 7. Обновляем статус наряда и заявки
                task.Status = "DiagnosticCompleted";
                task.CompletedAt = DateTime.UtcNow;

                if (task.RequestEntity != null)
                {
                    task.RequestEntity.Status = RequestStatus.DiagnosticCompleted;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Акт #{ActCode} полностью создан, заявка обновлена", act.ActCode);

                return new DiagnosticActResponseDto
                {
                    Success = true,
                    Message = "Акт создан и отправлен диспетчеру",
                    ActCode = act.ActCode
                };
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "❌ Ошибка БД при сохранении акта");
                _logger.LogError("💥 Inner: {Inner}", dbEx.InnerException?.Message);

                return new DiagnosticActResponseDto
                {
                    Success = false,
                    Message = $"Ошибка БД: {dbEx.InnerException?.Message ?? dbEx.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка создания акта");
                return new DiagnosticActResponseDto { Success = false, Message = ex.Message };
            }
        }

        // === Получение актов для диспетчера ===
        public async Task<List<DiagnosticActDto>> GetActsForDispatcherAsync(int? requestId = null, DiagnosticActStatus? status = null)
        {
            var query = _context.DiagnosticActs
                .Include(a => a.Task)
                    .ThenInclude(t => t!.RequestEntity!.ClientEntity)
                .Include(a => a.Task)
                    .ThenInclude(t => t!.StafEntity)
                .Include(a => a.SpareParts)
                    .ThenInclude(s => s.Spare)
                .Include(a => a.Works)
                    .ThenInclude(w => w.Work)
                .AsQueryable();

            if (requestId.HasValue)
                query = query.Where(a => a.Task!.RequestId == requestId.Value);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            var acts = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
            return acts.Select(MapToDto).ToList();
        }

        // === Получение акта для диспетчера по ID ===
        public async Task<DiagnosticActDto?> GetActByIdForDispatcherAsync(int actCode)
        {
            var act = await _context.DiagnosticActs
                .Include(a => a.Task)
                    .ThenInclude(t => t!.RequestEntity!.ClientEntity)
                .Include(a => a.Task)
                    .ThenInclude(t => t!.StafEntity)
                .Include(a => a.SpareParts)
                    .ThenInclude(s => s.Spare)
                .Include(a => a.Works)
                    .ThenInclude(w => w.Work)
                .FirstOrDefaultAsync(a => a.ActCode == actCode);

            return act == null ? null : MapToDto(act);
        }

        // === Отправка акта клиенту (диспетчер) ===
        // === Отправка акта клиенту (диспетчер) ===
        public async Task<bool> SubmitActToClientAsync(int actCode, int dispatcherTabNum, SubmitActToClientRequest request)
        {
            _logger.LogInformation("📤 Диспетчер #{Dispatcher} отправляет акт #{ActCode} клиенту", dispatcherTabNum, actCode);

            // 🔥 ДОБАВЬТЕ .Include() для загрузки связанных данных!
            var act = await _context.DiagnosticActs
                .Include(a => a.Works)        // ✅ Загружаем работы
                .Include(a => a.SpareParts)   // ✅ Загружаем ЗИП
                .FirstOrDefaultAsync(a => a.ActCode == actCode);

            if (act == null)
            {
                _logger.LogWarning("⚠️ Акт #{ActCode} не найден", actCode);
                return false;
            }

            if (act.Status != DiagnosticActStatus.Submitted)
            {
                _logger.LogWarning("⚠️ Акт #{ActCode} имеет статус {Status}, нельзя отправить клиенту", actCode, act.Status);
                return false;
            }

            act.Status = DiagnosticActStatus.SentToClient;
            act.SentToClientAt = DateTime.UtcNow;
            act.DispatcherComment = request.Comment;

            // 🔥 Теперь эти циклы безопасны, т.к. коллекции загружены:
            if (act.Works != null)
            {
                foreach (var w in act.Works)
                    w.IsApprovedByClient = null;
            }

            if (act.SpareParts != null)
            {
                foreach (var s in act.SpareParts)
                    s.IsApprovedByClient = null;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Акт #{ActCode} отправлен клиенту", actCode);
            return true;
        }

        // === Получение акта для клиента ===
        // Diploma.Application/Services/DiagnosticActService.cs

        public async Task<DiagnosticActDto?> GetActForClientAsync(int actCode, int clientId)
        {
            _logger.LogInformation("🔍 Запрос акта #{ActCode} для клиента #{ClientId}", actCode, clientId);

            // 🔥 ИСПРАВЛЕНО: добавлены все необходимые .ThenInclude()
            var act = await _context.DiagnosticActs
                .Include(a => a.Task)
                    .ThenInclude(t => t!.RequestEntity)
                        .ThenInclude(r => r.ClientEntity)
                            .ThenInclude(c => c.User)  // 🔥 Для FullName
                .Include(a => a.Task)
                    .ThenInclude(t => t!.StafEntity)
                        .ThenInclude(s => s.User)  // 🔥 Для ФИО инженера
                .Include(a => a.Works)
                    .ThenInclude(w => w.Work)
                .Include(a => a.SpareParts)
                    .ThenInclude(s => s.Spare)
                .FirstOrDefaultAsync(a => a.ActCode == actCode);

            if (act == null)
            {
                _logger.LogWarning("⚠️ Акт #{ActCode} не найден в БД", actCode);
                return null;
            }

            _logger.LogInformation("✅ Акт найден: Status={Status}, TaskId={TaskId}, RequestId={RequestId}",
                act.Status, act.TaskId, act.Task?.RequestId);

            if (act.Task?.RequestId == null)
            {
                _logger.LogWarning("⚠️ У акта #{ActCode} нет Task или RequestId", actCode);
                return null;
            }

            // Проверка принадлежности клиенту
            var req = await _context.Requests
                .FirstOrDefaultAsync(r => r.Id == act.Task.RequestId && r.ClientId == clientId);

            if (req == null)
            {
                _logger.LogWarning("⚠️ Заявка #{RequestId} не найдена или не принадлежит клиенту #{ClientId}",
                    act.Task.RequestId, clientId);
                return null;
            }

            _logger.LogInformation("✅ Заявка принадлежит клиенту");

            // Проверка статуса
            if (act.Status != DiagnosticActStatus.SentToClient &&
                act.Status != DiagnosticActStatus.ApprovedByClient &&
                act.Status != DiagnosticActStatus.RejectedByClient)
            {
                _logger.LogWarning("⚠️ Акт #{ActCode} имеет недопустимый статус {Status} для просмотра клиентом",
                    actCode, act.Status);
                return null;
            }

            _logger.LogInformation("✅ Акт #{ActCode} доступен для клиента", actCode);
            return MapToDto(act);
        }

        // === Согласование акта клиентом ===
        // Diploma.Application/Services/DiagnosticActService.cs

        public async Task<ActDecisionResponseDto> SubmitClientDecisionAsync(int actCode, int clientId, ClientActDecisionRequest request)
        {
            var act = await _context.DiagnosticActs
                .Include(a => a.SpareParts)
                .Include(a => a.Works)
                    .ThenInclude(w => w.Work)
                .Include(a => a.Task)
                    .ThenInclude(t => t!.RequestEntity)  // 🔥 Загружаем заявку
                .FirstOrDefaultAsync(a => a.ActCode == actCode);

            if (act == null || act.Task?.RequestId == null)
                return new ActDecisionResponseDto { Success = false, Message = "Акт не найден" };

            var req = await _context.Requests.FirstOrDefaultAsync(r => r.Id == act.Task.RequestId && r.ClientId == clientId);
            if (req == null)
                return new ActDecisionResponseDto { Success = false, Message = "Нет доступа" };

            if (act.Status != DiagnosticActStatus.SentToClient)
                return new ActDecisionResponseDto { Success = false, Message = "Акт ещё не отправлен на согласование" };

            // 🔥 Шаг 1: Обрабатываем решения по работам
            var rejectedWorkCodes = new List<int>();

            foreach (var aw in request.ApprovedWorks)
            {
                var work = act.Works.FirstOrDefault(w => w.WorkCode == aw.RecordId);
                if (work != null)
                {
                    work.IsApprovedByClient = aw.IsApproved;
                    if (!aw.IsApproved)
                    {
                        rejectedWorkCodes.Add(work.WorkCode);
                        _logger.LogInformation("🔴 Работа #{WorkCode} отклонена клиентом", work.WorkCode);
                    }
                }
            }

            // 🔥 Шаг 2: Автоматически отклоняем ЗИП, связанные с отклонёнными работами
            if (rejectedWorkCodes.Any())
            {
                var workSpareRelations = await _context.WorkSpareRelationsInAct
                    .Where(r => rejectedWorkCodes.Contains(r.WorkCode))
                    .ToListAsync();

                foreach (var relation in workSpareRelations)
                {
                    var spareInAct = act.SpareParts.FirstOrDefault(s => s.SpareCode == relation.SpareCode);
                    if (spareInAct != null && relation.IsRequired && spareInAct.IsApprovedByClient != false)
                    {
                        spareInAct.IsApprovedByClient = false;
                        _logger.LogInformation("🔴 ЗИП #{SpareCode} автоматически отклонён", relation.SpareCode);
                    }
                }
            }

            // 🔥 Шаг 3: Обрабатываем явные решения по ЗИП
            foreach (var as_ in request.ApprovedSpares)
            {
                var spare = act.SpareParts.FirstOrDefault(s => s.SpareCode == as_.RecordId);
                if (spare != null)
                {
                    spare.IsApprovedByClient = as_.IsApproved;
                    _logger.LogInformation("✅ ЗИП #{SpareCode}: решение = {Decision}", as_.RecordId, as_.IsApproved);
                }
            }

            // 🔥 Шаг 4: Определяем итоговый статус
            var hasApprovedWorks = act.Works.Any(w => w.IsApprovedByClient == true);
            var hasApprovedSpares = act.SpareParts.Any(s => s.IsApprovedByClient == true);
            var allRejected = !hasApprovedWorks && !hasApprovedSpares;

            if (allRejected)
            {
                act.Status = DiagnosticActStatus.RejectedByClient;
                act.ClientComment = request.Comment ?? "Отказ от всех работ и запчастей";

                // 🔥 ОБНОВЛЯЕМ СТАТУС ЗАЯВКИ: отклонена
                if (act.Task?.RequestEntity != null)
                {
                    act.Task.RequestEntity.Status = RequestStatus.Rejected;  // = 2
                }
            }
            else
            {
                act.Status = DiagnosticActStatus.ApprovedByClient;
                act.ApprovedByClientAt = DateTime.UtcNow;
                act.ClientComment = request.Comment;

                // 🔥 ОБНОВЛЯЕМ СТАТУС ЗАЯВКИ: согласована клиентом
                if (act.Task?.RequestEntity != null)
                {
                    act.Task.RequestEntity.Status = RequestStatus.ApprovedByClient;  // = 6 ✅
                }
            }

            act.UpdatedAt = DateTime.UtcNow;

            // 🔥 Сохраняем изменения: акт + заявка
            await _context.SaveChangesAsync();

            var approvedCost = await CalculateApprovedCostAsync(actCode);

            _logger.LogInformation("✅ Решение клиента обработано: акт #{ActCode}, заявка #{RequestId}, новый статус заявки = {RequestStatus}",
                actCode, act.Task?.RequestId, act.Task?.RequestEntity?.Status);

            return new ActDecisionResponseDto
            {
                Success = true,
                Message = allRejected ? "Все работы и запчасти отклонены" : "Решение сохранено",
                ApprovedCost = approvedCost,
                NewStatus = act.Status
            };
        }

        // === Вспомогательные методы ===
        private async Task<decimal?> CalculateApprovedCostAsync(int actCode)
        {
            var approvedWorks = await _context.WorkDiagnosticActs
                .Include(w => w.Work)
                .Where(w => w.ActCode == actCode && w.IsApprovedByClient == true)
                .ToListAsync();

            decimal total = 0;
            foreach (var w in approvedWorks)
            {
                if (w.Work?.PricePerHour != null)
                    total += w.Work.PricePerHour * 1; // Quantity пока 1
            }
            return total > 0 ? total : null;
        }

        private DiagnosticActDto MapToDto(DiagnosticAct act)
        {
            // 🔥 1. Загружаем связи работа-ЗИП из таблицы связей
            var workSpareRelations = _context.WorkSpareRelationsInAct
                .Include(r => r.Spare)
                .Where(r => r.ActCode == act.ActCode)
                .ToList();

            // 🔥 2. Загружаем ВСЕ заявки на ЗИП (для получения цен)
            var allSpareOrders = _context.SpareOrders
                .Include(o => o.SupplierSpare)
                    .ThenInclude(ss => ss.Spare)
                .ToList();

            // 🔥 3. Загружаем остатки на складе
            var spareStock = _context.Spares
                .Where(s => s.Amount > 0)
                .ToList()
                .ToDictionary(s => s.SpareCode, s => s.Amount);

            // 🔥 4. 🔥 ИСПРАВЛЕНО: Загружаем списанные количества с навигацией
            var writtenOff = _context.Set<SparePartsWriteOffEntity>()
                .Include(w => w.SupplierSpare)  // 🔥 Добавлено!
                .Where(w => w.TaskEntityId == act.TaskId)
                .GroupBy(w => w.SupplierSpareId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(w => w.Quantity)
                );

            // 🔥 5. Загружаем предложения поставщиков
            var supplierSpares = _context.SupplierSpares
                .Include(ss => ss.Supplier)
                .Include(ss => ss.Spare)
                .ToList();

            // 🔥 6. Вспомогательная функция для получения цены ЗИП
            decimal? GetSparePrice(int spareCode, int requiredQuantity)
            {
                // 🔥 Проверяем доступность на складе
                var totalInStock = spareStock.TryGetValue(spareCode, out var stock) ? stock : 0;
                var totalWrittenOff = writtenOff
                    .Where(kv => supplierSpares.FirstOrDefault(ss => ss.Id == kv.Key)?.SpareCode == spareCode)
                    .Sum(kv => kv.Value);

                var availableInStock = Math.Max(0, totalInStock - totalWrittenOff);
                var isAvailableInWarehouse = availableInStock >= requiredQuantity;

                if (isAvailableInWarehouse)
                {
                    // ✅ ЗИП ЕСТЬ НА СКЛАДЕ:
                    // 1. Сначала пробуем цену из заявки со статусом 1 (резерв)
                    var reservedOrder = allSpareOrders
                        .FirstOrDefault(o =>
                            o.SupplierSpare.SpareCode == spareCode &&
                            (int)o.Status == 1);

                    if (reservedOrder?.SupplierSpare?.UnitPrice != null)
                    {
                        _logger.LogDebug("  ✅ Цена из резерва (статус 1): {Price}",
                            reservedOrder.SupplierSpare.UnitPrice);
                        return reservedOrder.SupplierSpare.UnitPrice;
                    }

                    // 2. Если нет резерва — берём цену из SupplierSpares
                    var supplierSpare = supplierSpares
                        .FirstOrDefault(ss => ss.SpareCode == spareCode && ss.QuantityInStock > 0);

                    _logger.LogDebug("  ✅ Цена из SupplierSpares: {Price}",
                        supplierSpare?.UnitPrice);
                    return supplierSpare?.UnitPrice;
                }
                else
                {
                    // ❌ ЗИП НЕТ НА СКЛАДЕ:
                    // Применяем жадный алгоритм: ищем лучшего поставщика
                    var bestSupplier = supplierSpares
                        .Where(ss => ss.SpareCode == spareCode && ss.QuantityInStock > 0)
                        .OrderBy(ss => ss.UnitPrice)  // Сортируем по цене (дешевле first)
                        .FirstOrDefault();

                    _logger.LogDebug("  ✅ Цена от поставщика (жадный алгоритм): {Price}",
                        bestSupplier?.UnitPrice);
                    return bestSupplier?.UnitPrice;
                }
            }

            // 🔥 7. Группируем ЗИП по коду работы
            var workSparesMap = workSpareRelations
                .GroupBy(r => r.WorkCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r =>
                    {
                        var unitPrice = GetSparePrice(r.SpareCode, r.Quantity);

                        return new DiagnosticActSpareItemDto
                        {
                            RecordId = r.SpareCode,
                            SpareCode = r.SpareCode,
                            SpareName = r.Spare?.SpareName ?? $"ЗИП #{r.SpareCode}",
                            Quantity = r.Quantity,
                            IsApprovedByClient = act.SpareParts
                                .FirstOrDefault(s => s.SpareCode == r.SpareCode)?.IsApprovedByClient,
                            UnitPrice = unitPrice,
                            AvailableInWarehouse = spareStock.TryGetValue(r.SpareCode, out var stock) ? stock : 0,
                            Source = unitPrice != null &&
                                    spareStock.TryGetValue(r.SpareCode, out var s) && s >= r.Quantity
                                ? "Warehouse"
                                : "Supplier"
                        };
                    }).ToList()
                );

            // 🔥 8. Получаем коды ЗИП, которые привязаны к работам
            var linkedSpareCodes = new HashSet<int>(
                workSpareRelations.Select(r => r.SpareCode)
            );

            return new DiagnosticActDto
            {
                ActCode = act.ActCode,
                TaskId = act.TaskId,
                RequestId = act.Task?.RequestId ?? 0,
                ClientFio = act.Task?.RequestEntity?.ClientEntity?.User?.FullName ?? string.Empty,
                ClientPhone = act.Task?.RequestEntity?.ClientEntity?.Phone ?? string.Empty,
                EngineerFio = act.Task?.StafEntity?.User?.FullName ?? string.Empty,
                EngineerTabNum = act.Task?.ServiceEngineerTabNumber ?? 0,
                DiagnosticDate = act.DateOnly,
                DiagnosticTime = act.TimeOnly,
                ExternalCondition = act.ExternalCondition,
                IdentifiedIssues = act.IdentifiedIssues,
                TestResults = act.TestResults,
                Recommendations = act.Recommendations,
                EstimatedCost = act.EstimatedCost,
                EstimatedTime = act.EstimatedTime,
                Status = act.Status,
                DispatcherComment = act.DispatcherComment,
                ClientComment = act.ClientComment,
                SubmittedToDispatcherAt = act.SubmittedToDispatcherAt,
                SentToClientAt = act.SentToClientAt,
                ApprovedByClientAt = act.ApprovedByClientAt,
                CreatedAt = act.CreatedAt,

                // 🔥 9. Работы с привязанными ЗИП и ценами
                Works = act.Works.Select(w => new DiagnosticActWorkItemDto
                {
                    RecordId = w.WorkCode,
                    WorkCode = w.WorkCode,
                    WorkName = w.Work?.WorkName ?? $"Работа #{w.WorkCode}",
                    Quantity = 1,
                    EstimatedCost = w.Work?.PricePerHour,
                    IsApprovedByClient = w.IsApprovedByClient,
                    RequiredSpares = workSparesMap.TryGetValue(w.WorkCode, out var spares)
                        ? spares
                        : new List<DiagnosticActSpareItemDto>()
                }).ToList(),

                // 🔥 10. Общие ЗИП (не привязанные к работам) с ценами
                Spares = act.SpareParts
                    .Where(s => !linkedSpareCodes.Contains(s.SpareCode))
                    .Select(s =>
                    {
                        var unitPrice = GetSparePrice(s.SpareCode, s.Amount);

                        return new DiagnosticActSpareItemDto
                        {
                            SpareCode = s.SpareCode,
                            SpareName = s.Spare?.SpareName ?? $"ЗИП #{s.SpareCode}",
                            Quantity = s.Amount,
                            IsApprovedByClient = s.IsApprovedByClient,
                            UnitPrice = unitPrice,
                            AvailableInWarehouse = spareStock.TryGetValue(s.SpareCode, out var stock) ? stock : 0,
                            Source = unitPrice != null &&
                                    spareStock.TryGetValue(s.SpareCode, out var st) && st >= s.Amount
                                ? "Warehouse"
                                : "Supplier"
                        };
                    }).ToList()
            };
        }
        //private DiagnosticActDto MapToDto(DiagnosticAct act)
        //{
        //    // 🔥 1. Загружаем связи работа-ЗИП из таблицы связей
        //    var workSpareRelations = _context.WorkSpareRelationsInAct
        //        .Include(r => r.Spare)
        //        .Where(r => r.ActCode == act.ActCode)
        //        .ToList();

        //    // 🔥 2. Загружаем заявки на ЗИП (бронь) для получения цен
        //    var spareOrders = _context.SpareOrders
        //        .Include(o => o.SupplierSpare)
        //            .ThenInclude(ss => ss.Spare)
        //        .Where(o => o.Status == SpareOrderStatus.Reservation)
        //        .ToList();

        //    // 🔥 3. Создаём словарь: SpareCode -> цена из заявки
        //    var sparePrices = spareOrders
        //        .GroupBy(o => o.SupplierSpare.SpareCode)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.FirstOrDefault()?.SupplierSpare?.UnitPrice
        //        );

        //    // 🔥 4. Группируем ЗИП по коду работы
        //    var workSparesMap = workSpareRelations
        //        .GroupBy(r => r.WorkCode)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.Select(r => new DiagnosticActSpareItemDto
        //            {
        //                RecordId = r.SpareCode,
        //                SpareCode = r.SpareCode,
        //                SpareName = r.Spare?.SpareName ?? $"ЗИП #{r.SpareCode}",
        //                Quantity = r.Quantity,
        //                IsApprovedByClient = act.SpareParts
        //                    .FirstOrDefault(s => s.SpareCode == r.SpareCode)?.IsApprovedByClient,
        //                // 🔥 Берём цену из заявок на ЗИП
        //                UnitPrice = sparePrices.TryGetValue(r.SpareCode, out var price) ? price : null
        //            }).ToList()
        //        );

        //    // 🔥 5. Получаем коды ЗИП, которые привязаны к работам
        //    var linkedSpareCodes = new HashSet<int>(
        //        workSpareRelations.Select(r => r.SpareCode)
        //    );

        //    return new DiagnosticActDto
        //    {
        //        ActCode = act.ActCode,
        //        TaskId = act.TaskId,
        //        RequestId = act.Task?.RequestId ?? 0,
        //        ClientFio = act.Task?.RequestEntity?.ClientEntity?.FIO ?? string.Empty,
        //        ClientPhone = act.Task?.RequestEntity?.ClientEntity?.Phone ?? string.Empty,
        //        EngineerFio = act.Task?.StafEntity?.FIO ?? string.Empty,
        //        EngineerTabNum = act.Task?.ServiceEngineerTabNumber ?? 0,
        //        DiagnosticDate = act.DateOnly,
        //        DiagnosticTime = act.TimeOnly,
        //        ExternalCondition = act.ExternalCondition,
        //        IdentifiedIssues = act.IdentifiedIssues,
        //        TestResults = act.TestResults,
        //        Recommendations = act.Recommendations,
        //        EstimatedCost = act.EstimatedCost,
        //        EstimatedTime = act.EstimatedTime,
        //        Status = act.Status,
        //        DispatcherComment = act.DispatcherComment,
        //        ClientComment = act.ClientComment,
        //        SubmittedToDispatcherAt = act.SubmittedToDispatcherAt,
        //        SentToClientAt = act.SentToClientAt,
        //        ApprovedByClientAt = act.ApprovedByClientAt,
        //        CreatedAt = act.CreatedAt,

        //        // 🔥 6. Работы с привязанными ЗИП и ценами
        //        Works = act.Works.Select(w => new DiagnosticActWorkItemDto
        //        {
        //            RecordId = w.WorkCode,
        //            WorkCode = w.WorkCode,
        //            WorkName = w.Work?.WorkName ?? $"Работа #{w.WorkCode}",
        //            Quantity = 1,
        //            EstimatedCost = w.Work?.PricePerHour,
        //            IsApprovedByClient = w.IsApprovedByClient,
        //            RequiredSpares = workSparesMap.TryGetValue(w.WorkCode, out var spares)
        //                ? spares
        //                : new List<DiagnosticActSpareItemDto>()
        //        }).ToList(),

        //        // 🔥 7. Общие ЗИП (не привязанные к работам) с ценами
        //        Spares = act.SpareParts
        //            .Where(s => !linkedSpareCodes.Contains(s.SpareCode))
        //            .Select(s => new DiagnosticActSpareItemDto
        //            {
        //                RecordId = s.SpareCode,
        //                SpareCode = s.SpareCode,
        //                SpareName = s.Spare?.SpareName ?? $"ЗИП #{s.SpareCode}",
        //                Quantity = s.Amount,
        //                IsApprovedByClient = s.IsApprovedByClient,
        //                // 🔥 Берём цену из заявок на ЗИП
        //                UnitPrice = sparePrices.TryGetValue(s.SpareCode, out var price) ? price : null
        //            }).ToList()
        //    };
        //}

        public Task<DiagnosticAct?> GetActByTaskIdAsync(int taskId)
        {
            return _context.DiagnosticActs.FirstOrDefaultAsync(a => a.TaskId == taskId);
        }

        public async Task<List<DiagnosticActDto>> GetActsForClientPendingAsync(int clientId)
        {
            _logger.LogInformation("📋 Получение ожидающих актов для клиента #{ClientId}", clientId);

            var acts = await _context.DiagnosticActs
                .Include(a => a.Task)
                    .ThenInclude(t => t!.RequestEntity)
                .Include(a => a.SpareParts)
                    .ThenInclude(s => s.Spare)
                .Include(a => a.Works)
                    .ThenInclude(w => w.Work)
                .Where(a => a.Task!.RequestId != null &&
                           a.Task.RequestEntity!.ClientId == clientId &&
                           (a.Status == DiagnosticActStatus.SentToClient ||
                            a.Status == DiagnosticActStatus.ApprovedByClient ||
                            a.Status == DiagnosticActStatus.RejectedByClient))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return acts.Select(MapToDto).ToList();
        }


    }


}
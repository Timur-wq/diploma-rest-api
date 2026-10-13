using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diploma.Core.Abstractions;

namespace Diploma.DataAccess.Entities.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<TaskRepository> _logger;

        public TaskRepository(DiplomaDbContext context, ILogger<TaskRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TaskEntity?> AssignWorkOrderAsync(int workId, int requestId, int maxActiveOrders = 2)
        {
            _logger.LogInformation("🔍 Поиск работы WorkId={WorkId}", workId);

            // 0. Получить информацию о работе
            var work = await _context.WorkEntities
                .Include(w => w.WorkClass)
                .FirstOrDefaultAsync(w => w.WorkCode == workId);

            if (work == null)
            {
                _logger.LogError("❌ Работа с кодом {WorkId} не найдена", workId);
                return null;
            }

            _logger.LogInformation("✅ Работа найдена: {WorkName}, ClassCode={ClassCode}",
                work.WorkName, work.WorkClassCode);

            // Получить информацию о заявке
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                _logger.LogError("❌ Заявка с кодом {RequestId} не найдена", requestId);
                return null;
            }

            _logger.LogInformation("✅ Заявка найдена: Model={Model}, Type={Type}",
                request.Model, request.SvtType);

            // ============================================
            // ЭТАП 1: Фильтрация по квалификации
            // ============================================
            _logger.LogInformation("========== ЭТАП 1: КВАЛИФИКАЦИЯ ==========");

            // 1.1. Найти сертификаты, которые поддерживают данную модель СВТ
            _logger.LogInformation("🔍 Поиск сертификатов для модели: {Model}", request.Model);

            var eligibleCertificateIds = await (
                from cm in _context.Set<CertificateModelEntity>()
                where cm.Model.ModelName == request.Model
                select cm.CertificateId
            ).ToListAsync();

            _logger.LogInformation("📋 Найдено сертификатов для модели: {Count}", eligibleCertificateIds.Count);
            foreach (var certId in eligibleCertificateIds)
            {
                _logger.LogInformation("  📜 Сертификат #{CertId}", certId);
            }

            if (!eligibleCertificateIds.Any())
            {
                _logger.LogWarning("⚠️ Нет сертификатов для модели {Model}", request.Model);
            }

            // 1.2. Найти сотрудников с активными сертификатами на эту модель
            var qualifiedStaffIds = await (
                from sc in _context.StafCertificates
                where eligibleCertificateIds.Contains(sc.CertificateId)
                  && sc.Status == "active"
                  && (!sc.ExpirationDate.HasValue || sc.ExpirationDate.Value > DateTime.UtcNow)
                select sc.StafId
            ).ToListAsync();

            _logger.LogInformation("📋 Сотрудников с активными сертификатами: {Count}", qualifiedStaffIds.Count);
            foreach (var staffId in qualifiedStaffIds)
            {
                _logger.LogInformation("  👷 Сотрудник #{StaffId}", staffId);
            }

            // 1.3. Найти сотрудников с допуском к классу работ
            _logger.LogInformation("🔍 Поиск сотрудников с допуском к классу работ: {ClassCode}", work.WorkClassCode);

            var accessStaffIds = await (
                from wca in _context.WorkClassAccessEntities
                where wca.WorkClassCode == work.WorkClassCode
                  && wca.StafCertificate.Status == "active"
                select wca.StafCertificate.StafId
            ).ToListAsync();

            _logger.LogInformation("📋 Сотрудников с допуском: {Count}", accessStaffIds.Count);
            foreach (var staffId in accessStaffIds)
            {
                _logger.LogInformation("  👷 Сотрудник #{StaffId}", staffId);
            }

            // Пересечение: сотрудники с И сертификатом, И допуском
            var eligibleIds = qualifiedStaffIds.Intersect(accessStaffIds).ToList();

            _logger.LogInformation("✅ Квалифицированных сотрудников (пересечение): {Count}", eligibleIds.Count);
            foreach (var staffId in eligibleIds)
            {
                _logger.LogInformation("  👷 Сотрудник #{StaffId}", staffId);
            }

            if (!eligibleIds.Any())
            {
                _logger.LogWarning("⚠️ НЕТ сотрудников с нужной квалификацией!");
                _logger.LogWarning("   - Сертификатов для модели: {CertCount}", eligibleCertificateIds.Count);
                _logger.LogWarning("   - Сотрудников с сертификатами: {StaffCertCount}", qualifiedStaffIds.Count);
                _logger.LogWarning("   - Сотрудников с допуском: {StaffAccessCount}", accessStaffIds.Count);
                return null;
            }

            // ============================================
            // ЭТАП 2: Фильтрация по загрузке
            // ============================================
            _logger.LogInformation("========== ЭТАП 2: ЗАГРУЗКА ==========");
            _logger.LogInformation("📊 Лимит активных нарядов: {Limit}", maxActiveOrders);

            var staffWithLoad = new List<(int TabNum, int ActiveOrders)>();

            foreach (var staffId in eligibleIds)
            {
                var activeCount = await _context.TaskEntities
                    .CountAsync(t =>
                        t.ServiceEngineerTabNumber == staffId &&
                        (t.Status == "assigned" || t.Status == "in_progress"));

                _logger.LogInformation("  👷 Сотрудник #{StaffId}: активных нарядов = {Count}",
                    staffId, activeCount);

                if (activeCount < maxActiveOrders)
                {
                    staffWithLoad.Add((staffId, activeCount));
                    _logger.LogInformation("    ✅ Проходит фильтр загрузки");
                }
                else
                {
                    _logger.LogInformation("    ❌ Превышен лимит ({Active} >= {Limit})",
                        activeCount, maxActiveOrders);
                }
            }

            if (!staffWithLoad.Any())
            {
                _logger.LogWarning("⚠️ ВСЕ квалифицированные сотрудники перегружены!");
                return null;
            }

            // ============================================
            // ЭТАП 3: Ранжирование
            // ============================================
            _logger.LogInformation("========== ЭТАП 3: РАНЖИРОВАНИЕ ==========");

            var selectedStaffId = staffWithLoad
                .OrderBy(s => s.ActiveOrders)
                .ThenBy(s => s.TabNum)
                .First().TabNum;

            _logger.LogInformation("✅ Выбран сотрудник #{StaffId} (загрузка: {Load})",
                selectedStaffId, staffWithLoad.First(s => s.TabNum == selectedStaffId).ActiveOrders);

            // ============================================
            // ЭТАП 4: Создание наряда
            // ============================================
            _logger.LogInformation("========== ЭТАП 4: СОЗДАНИЕ НАРЯДА ==========");

            var newTask = new TaskEntity
            {
                ServiceEngineerTabNumber = selectedStaffId,
                WorkId = workId,
                RequestId = requestId,
                AssignedAt = DateTime.UtcNow,
                Status = "assigned",
                StartedAt = null,
                CompletedAt = null
            };

            _context.TaskEntities.Add(newTask);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Наряд создан: TaskId={TaskId}", newTask.WorkOrderId);
            _logger.LogInformation("========== НАЗНАЧЕНИЕ ЗАВЕРШЕНО ==========");

            return newTask;
        }

        public async Task<List<StafEntity>> GetEligibleEngineersAsync(int workId, string svtModel, int maxActiveOrders = 2)
        {
            _logger.LogInformation("🔍 GetEligibleEngineers: WorkId={WorkId}, Model={Model}", workId, svtModel);

            // 0. Получить работу
            var work = await _context.WorkEntities
                .Include(w => w.WorkClass)
                .FirstOrDefaultAsync(w => w.WorkCode == workId);

            if (work == null)
            {
                _logger.LogError("❌ Работа не найдена: WorkId={WorkId}", workId);
                return new List<StafEntity>();
            }

            _logger.LogInformation("✅ Работа найдена: {WorkName}, ClassCode={ClassCode}",
                work.WorkName, work.WorkClassCode);

            // ============================================
            // ЭТАП 1: Сертификаты для модели
            // ============================================
            _logger.LogInformation("========== ЭТАП 1: СЕРТИФИКАТЫ ДЛЯ МОДЕЛИ ==========");
            _logger.LogInformation("🔍 Поиск сертификатов для модели: {Model}", svtModel);

            var eligibleCertificateIds = await (
                from cm in _context.Set<CertificateModelEntity>()
                where cm.Model.ModelName == svtModel
                select cm.CertificateId
            ).ToListAsync();

            _logger.LogInformation("📋 Найдено сертификатов для модели: {Count}", eligibleCertificateIds.Count);
            foreach (var certId in eligibleCertificateIds)
            {
                _logger.LogInformation("  📜 Сертификат #{CertId}", certId);
            }

            if (!eligibleCertificateIds.Any())
            {
                _logger.LogWarning("⚠️ Нет сертификатов для модели {Model} в таблице 'Сертификаты-Модели'", svtModel);
                _logger.LogWarning("💡 Проверьте: SELECT * FROM \"Сертификаты-Модели\"");
                return new List<StafEntity>();
            }

            // ============================================
            // ЭТАП 2: Сотрудники с активными сертификатами
            // ============================================
            _logger.LogInformation("========== ЭТАП 2: СОТРУДНИКИ С СЕРТИФИКАТАМИ ==========");

            var qualifiedStaffIds = await (
                from sc in _context.StafCertificates
                where eligibleCertificateIds.Contains(sc.CertificateId)
                  && sc.Status == "active"
                  && (!sc.ExpirationDate.HasValue || sc.ExpirationDate.Value > DateTime.UtcNow)
                select sc.StafId
            ).ToListAsync();

            _logger.LogInformation("📋 Сотрудников с активными сертификатами: {Count}", qualifiedStaffIds.Count);
            foreach (var staffId in qualifiedStaffIds)
            {
                _logger.LogInformation("  👷 Сотрудник #{StaffId}", staffId);
            }

            if (!qualifiedStaffIds.Any())
            {
                _logger.LogWarning("⚠️ Нет сотрудников с активными сертификатами");
                _logger.LogWarning("💡 Проверьте: SELECT * FROM \"Сотрудники-Сертификаты\" WHERE \"Статус\"='active'");
                return new List<StafEntity>();
            }

            // ============================================
            // ЭТАП 3: Сотрудники с допуском к классу работ
            // ============================================
            _logger.LogInformation("========== ЭТАП 3: ДОПУСК К КЛАССУ РАБОТ ==========");
            _logger.LogInformation("🔍 Класс работ: {ClassCode}", work.WorkClassCode);

            var accessStaffIds = await (
                from wca in _context.WorkClassAccessEntities
                where wca.WorkClassCode == work.WorkClassCode
                  && wca.StafCertificate.Status == "active"
                select wca.StafCertificate.StafId
            ).ToListAsync();

            _logger.LogInformation("📋 Сотрудников с допуском: {Count}", accessStaffIds.Count);
            foreach (var staffId in accessStaffIds)
            {
                _logger.LogInformation("  👷 Сотрудник #{StaffId}", staffId);
            }

            if (!accessStaffIds.Any())
            {
                _logger.LogWarning("⚠️ Нет сотрудников с допуском к классу работ {ClassCode}", work.WorkClassCode);
                _logger.LogWarning("💡 Проверьте: SELECT * FROM \"Допуск к классам работ\"");
                return new List<StafEntity>();
            }

            // ============================================
            // ЭТАП 4: Пересечение (квалифицированные)
            // ============================================
            _logger.LogInformation("========== ЭТАП 4: ПЕРЕСЕЧЕНИЕ ==========");

            var eligibleIds = qualifiedStaffIds.Intersect(accessStaffIds).ToList();

            _logger.LogInformation("✅ Квалифицированных сотрудников (пересечение): {Count}", eligibleIds.Count);
            foreach (var staffId in eligibleIds)
            {
                _logger.LogInformation("  👷 Сотрудник #{StaffId}", staffId);
            }

            if (!eligibleIds.Any())
            {
                _logger.LogWarning("⚠️ НЕТ пересечения между сертификатами и допуском!");
                _logger.LogWarning("   - Сотрудников с сертификатами: {CertCount}", qualifiedStaffIds.Count);
                _logger.LogWarning("   - Сотрудников с допуском: {AccessCount}", accessStaffIds.Count);
                _logger.LogWarning("💡 Проверьте связи в таблице 'Допуск к классам работ'");
                return new List<StafEntity>();
            }

            // ============================================
            // ЭТАП 5: Фильтрация по загрузке
            // ============================================
            _logger.LogInformation("========== ЭТАП 5: ЗАГРУЗКА ==========");
            _logger.LogInformation("📊 Лимит активных нарядов: {Limit}", maxActiveOrders);

            var result = new List<StafEntity>();

            foreach (var staffId in eligibleIds)
            {
                var activeCount = await _context.TaskEntities
                    .CountAsync(t =>
                        t.ServiceEngineerTabNumber == staffId &&
                        (t.Status == "assigned" || t.Status == "in_progress"));

                _logger.LogInformation("  👷 Сотрудник #{StaffId}: активных нарядов = {Count}",
                    staffId, activeCount);

                if (activeCount < maxActiveOrders)
                {
                    // Стало:
                    var staff = await _context.Stafs
                        .Include(s => s.User)  // 🔥 Загружаем пользователя!
                        .FirstOrDefaultAsync(s => s.TabNum == staffId && s.User.IsActive);

                    if (staff != null)
                    {
                        result.Add(staff);
                        //_logger.LogInformation("    ✅ ДОБАВЛЕН: {FIO}", staff.User.FullName);
                    }
                    else
                    {
                        _logger.LogInformation("    ❌ Сотрудник не найден или не активен");
                    }
                }
                else
                {
                    _logger.LogInformation("    ❌ Превышен лимит ({Active} >= {Limit})",
                        activeCount, maxActiveOrders);
                }
            }

            // ============================================
            // ИТОГ
            // ============================================
            _logger.LogInformation("========== ИТОГ ==========");
            _logger.LogInformation("✅ Найдено кандидатов: {Count}", result.Count);

            if (result.Any())
            {
                foreach (var candidate in result)
                {
                    //_logger.LogInformation("  👷 Сотрудник #{TabNum}: {FIO}, Роль={Role}",
                    //    candidate.TabNum, candidate.FIO, candidate.Role);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ КАНАДИДАТЫ НЕ НАЙДЕНЫ");
                _logger.LogWarning("💡 Возможные причины:");
                _logger.LogWarning("   1. Нет сертификатов для модели '{Model}'", svtModel);
                _logger.LogWarning("   2. Нет сотрудников с активными сертификатами");
                _logger.LogWarning("   3. Нет допуска к классу работ {ClassCode}", work.WorkClassCode);
                _logger.LogWarning("   4. Все сотрудники перегружены");
            }

            return result;
        }

        public async Task<int> GetActiveOrdersCountAsync(int staffTabNumber)
        {
            var count = await _context.TaskEntities
                .CountAsync(t =>
                    t.ServiceEngineerTabNumber == staffTabNumber &&
                    (t.Status == "assigned" || t.Status == "in_progress"));

            _logger.LogDebug("📊 Сотрудник #{StaffId}: активных нарядов = {Count}",
                staffTabNumber, count);

            return count;
        }

        public async Task<TaskEntity?> GetByIdAsync(int taskId)
        {
            return await _context.TaskEntities
                .Include(t => t.StafEntity)
                .Include(t => t.WorkEntity)
                .Include(t => t.RequestEntity)
                .FirstOrDefaultAsync(t => t.WorkOrderId == taskId);
        }

        public async Task<List<TaskEntity>> GetAllAsync()
        {
            return await _context.TaskEntities
                .Include(t => t.StafEntity)
                .Include(t => t.WorkEntity)
                .Include(t => t.RequestEntity)
                .ToListAsync();
        }

        public async Task<TaskEntity?> UpdateAsync(TaskEntity task)
        {
            _context.TaskEntities.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }

        // ============================================
        // 🔥 МЕТОД ДЛЯ РЕМОНТА (множество работ)
        // ============================================
        public async Task<List<TaskEntity>> AssignRepairWorkOrdersAsync(int requestId, int maxActiveOrders = 2)
        {
            _logger.LogInformation("🔧 Назначение нарядов на РЕМОНТ для заявки #{RequestId}", requestId);

            // 1. Находим акт диагностики по заявке
            var act = await _context.DiagnosticActs
                .Include(a => a.Works)
                    .ThenInclude(w => w.Work)
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

            if (act == null)
            {
                _logger.LogError("❌ Акт для заявки #{RequestId} не найден", requestId);
                return new List<TaskEntity>();
            }

            // 2. Проверяем, что есть согласованные клиентом работы
            var approvedWorks = act.Works
                .Where(w => w.IsApprovedByClient == true)
                .ToList();

            if (!approvedWorks.Any())
            {
                _logger.LogError("❌ В акте #{ActCode} нет согласованных работ", act.ActCode);
                return new List<TaskEntity>();
            }

            _logger.LogInformation("✅ В акте {Count} согласованных работ", approvedWorks.Count);
            foreach (var work in approvedWorks)
            {
                _logger.LogInformation("  - Работа #{WorkCode}: {WorkName}",
                    work.WorkCode, work.Work?.WorkName);
            }

            // 3. Получаем модель СВТ для подбора инженера
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                _logger.LogError("❌ Заявка #{RequestId} не найдена", requestId);
                return new List<TaskEntity>();
            }

            // 4. Подбираем инженера по ПЕРВОЙ работе (для всех работ один инженер)
            var firstWorkCode = approvedWorks.First().WorkCode;

            var eligibleEngineers = await GetEligibleEngineersAsync(
                firstWorkCode,
                request.Model,
                maxActiveOrders
            );

            if (!eligibleEngineers.Any())
            {
                _logger.LogWarning("⚠️ Нет доступных инженеров");
                return new List<TaskEntity>();
            }

            var selectedEngineer = eligibleEngineers.First();

            _logger.LogInformation("✅ Выбран инженер #{TabNum}: {FIO}",
                selectedEngineer.TabNum, selectedEngineer.User.FullName);

            // 5. 🔥 Создаём ОТДЕЛЬНЫЙ наряд на КАЖДУЮ согласованную работу
            var tasks = new List<TaskEntity>();

            foreach (var work in approvedWorks)
            {
                var newTask = new TaskEntity
                {
                    RequestId = requestId,
                    WorkId = work.WorkCode,  // ← Одна работа на наряд
                    ServiceEngineerTabNumber = selectedEngineer.TabNum,
                    AssignedAt = DateTime.UtcNow,
                    Status = "assigned",
                    StartedAt = null,
                    CompletedAt = null
                };

                _context.TaskEntities.Add(newTask);
                tasks.Add(newTask);

                _logger.LogInformation("📋 Создан наряд #{TaskId} на работу #{WorkCode}",
                    newTask.WorkOrderId, work.WorkCode);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Создано {Count} нарядов для заявки #{RequestId}",
                tasks.Count, requestId);

            return tasks;
        }
    }
}

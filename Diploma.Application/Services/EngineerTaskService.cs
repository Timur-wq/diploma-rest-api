using Diploma.Core.Abstractions;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Application.Services
{
    public class EngineerTaskService : IEngineerTaskService
    {
        private readonly IEngineerTaskRepository _repository;
        private readonly ILogger<EngineerTaskService> _logger;
        private readonly DiplomaDbContext _context;
        public EngineerTaskService(
            IEngineerTaskRepository repository,
            DiplomaDbContext context,
            ILogger<EngineerTaskService> logger)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
        }
        public async Task<List<EngineerTaskDto>> GetAllTasksAsync(int staffTabNumber)
        {
            _logger.LogInformation("📋 Получение всех нарядов для сотрудника #{TabNum}", staffTabNumber);

            var tasks = await _repository.GetAllTasksAsync(staffTabNumber);
            return tasks.Select(MapToDto).ToList();
        }

        public async Task<List<EngineerTaskDto>> GetActiveTasksAsync(int staffTabNumber)
        {
            _logger.LogInformation("📋 Получение активных нарядов для сотрудника #{TabNum}", staffTabNumber);

            var tasks = await _repository.GetActiveTasksAsync(staffTabNumber);
            return tasks.Select(MapToDto).ToList();
        }

        public async Task<List<EngineerTaskDto>> GetCompletedTasksAsync(int staffTabNumber)
        {
            _logger.LogInformation("📋 Получение завершённых нарядов для сотрудника #{TabNum}", staffTabNumber);

            var tasks = await _repository.GetCompletedTasksAsync(staffTabNumber);
            return tasks.Select(MapToDto).ToList();
        }

        public async Task<EngineerTaskDto?> GetTaskByIdAsync(int taskId, int staffTabNumber)
        {
            _logger.LogInformation("🔍 Поиск наряда #{TaskId} для сотрудника #{TabNum}", taskId, staffTabNumber);

            var task = await _repository.GetTaskByIdAsync(taskId, staffTabNumber);
            return task == null ? null : MapToDto(task);
        }

        public async Task<EngineerStatsDto> GetStatsAsync(int staffTabNumber)
        {
            return await _repository.GetStatsAsync(staffTabNumber);
        }

        public async Task<bool> StartTaskAsync(int taskId, int staffTabNumber)
        {
            _logger.LogInformation("🚀 Начало работы по наряду #{TaskId}", taskId);

            var task = await _repository.GetTaskByIdAsync(taskId, staffTabNumber);

            if (task == null)
            {
                _logger.LogWarning("⚠️ Наряд #{TaskId} не найден или не принадлежит сотруднику #{TabNum}",
                    taskId, staffTabNumber);
                return false;
            }

            var result = await _repository.StartTaskAsync(taskId);
            return result != null;
        }

        public async Task<bool> CompleteTaskAsync(int taskId, int staffTabNumber)
        {
            _logger.LogInformation("✅ Завершение работы по наряду #{TaskId}", taskId);

            var task = await _repository.GetTaskByIdAsync(taskId, staffTabNumber);

            if (task == null)
            {
                _logger.LogWarning("⚠️ Наряд #{TaskId} не найден или не принадлежит сотруднику #{TabNum}",
                    taskId, staffTabNumber);
                return false;
            }

            var result = await _repository.CompleteTaskAsync(taskId);
            return result != null;
        }

        public async Task<bool> CancelTaskAsync(int taskId, int staffTabNumber, string reason)
        {
            _logger.LogInformation("❌ Отмена наряда #{TaskId}", taskId);

            var task = await _repository.GetTaskByIdAsync(taskId, staffTabNumber);

            if (task == null)
            {
                _logger.LogWarning("⚠️ Наряд #{TaskId} не найден или не принадлежит сотруднику #{TabNum}",
                    taskId, staffTabNumber);
                return false;
            }

            var result = await _repository.CancelTaskAsync(taskId, reason);
            return result != null;
        }

        // 🔥 НОВЫЙ МЕТОД: Получение данных для генерации актов
        public async Task<RepairTaskPdfData?> GetTaskDetailsForPdfAsync(int taskId)
        {
            var task = await _context.TaskEntities
                .Include(t => t.RequestEntity)
                    .ThenInclude(r => r.ClientEntity)
                .Include(t => t.WorkEntity)
                .Include(t => t.StafEntity)
                .FirstOrDefaultAsync(t => t.WorkOrderId == taskId);

            if (task == null) return null;

            // Получаем согласованные ЗИП из акта
            var act = await _context.DiagnosticActs
                .Include(a => a.SpareParts)
                    .ThenInclude(sp => sp.Spare)
                .FirstOrDefaultAsync(a => a.Task!.RequestId == task.RequestId);

            var approvedSpares = act?.SpareParts
                .Where(sp => sp.IsApprovedByClient == true)
            .Select(sp =>
            {
                    var supplierSpare = _context.SupplierSpares
                        .FirstOrDefault(ss => ss.SpareCode == sp.SpareCode);

                    return new SpareItemData
                    {
                        SpareName = sp.Spare?.SpareName ?? $"ЗИП #{sp.SpareCode}",
                        Quantity = sp.Amount,
                        UnitPrice = supplierSpare?.UnitPrice ?? 0
                    };
                }).ToList() ?? new List<SpareItemData>();

            return new RepairTaskPdfData
            {
                TaskId = task.WorkOrderId,
                RequestId = task.RequestId,
                ClientFio = task.RequestEntity?.ClientEntity?.User?.FullName ?? "",
                DeviceModel = task.RequestEntity?.Model ?? "",
                SerialNumber = task.RequestEntity?.SerialNumber ?? "",
                WorkName = task.WorkEntity?.WorkName ?? "",
                EngineerFio = task.StafEntity?.User?.FullName ?? "",
                EngineerTabNumber = task.ServiceEngineerTabNumber,
                ApprovedSpares = approvedSpares
            };
        }

        // Diploma.Application/Services/EngineerTaskService.cs

        // Diploma.Application/Services/EngineerTaskService.cs

        // Diploma.Application/Services/EngineerTaskService.cs

        public async Task<bool> UpdateTaskDocumentsAsync(int taskId, string actPath, string writeOffPath)
        {
            var task = await _context.TaskEntities
                .Include(t => t.RequestEntity)
                .FirstOrDefaultAsync(t => t.WorkOrderId == taskId);

            if (task?.RequestEntity == null) return false;

            // 🔥 Получаем согласованные ЗИП из акта диагностики
            var diagnosticAct = await _context.DiagnosticActs
                .Include(a => a.SpareParts)
                    .ThenInclude(sp => sp.Spare)
                .FirstOrDefaultAsync(a => a.Task!.RequestId == task.RequestId);

            var approvedSpares = diagnosticAct?.SpareParts
                .Where(sp => sp.IsApprovedByClient == true)
                .ToList();

            // 🔥 Создаём Акт выполненных работ
            var repairAct = new RepairActEntity
            {
                TaskId = taskId,
                CreatedAt = DateTime.UtcNow,
                FilePath = actPath,
                Comment = $"Акт сформирован при завершении наряда #{taskId}"
            };

            // 🔥 Создаём запись в БД для акта выполненных работ
            _context.RepairActs.Add(repairAct);
            await _context.SaveChangesAsync();  // ← Сохраняем, чтобы получить ID

            // 🔥 Временное решение: ручная генерация ID
            var nextWriteOffId = await _context.SparePartsWriteOffEntities
                .MaxAsync(a => (int?)a.ActCode) ?? 0;

            // 🔥 Создаём отдельные записи акта списания для КАЖДОГО ЗИП
            foreach (var spare in approvedSpares)
            {
                // 🔥 Получаем SupplierSpareId из согласованного ЗИП
                var supplierSpare = await _context.SupplierSpares
                    .FirstOrDefaultAsync(ss => ss.SpareCode == spare.SpareCode);

                if (supplierSpare == null)
                {
                    _logger.LogWarning("⚠️ ЗИП #{SpareCode} не найден у поставщиков", spare.SpareCode);
                    continue;  // Пропускаем этот ЗИП
                }

                var writeOffAct = new SparePartsWriteOffEntity
                {
                    ActCode = ++nextWriteOffId,
                    TaskEntityId = taskId,
                    SupplierSpareId = supplierSpare.Id,  // 🔥 Реальный ID, а не 0!
                    Quantity = spare.Amount,
                    WriteOffType = WriteOffType.Installation,
                    WriteOffDate = DateTime.UtcNow,
                    FilePath = writeOffPath,
                    Comment = $"Списание для наряда #{taskId}"
                };

                _context.SparePartsWriteOffEntities.Add(writeOffAct);
            }

            await _context.SaveChangesAsync();

            return true;
        }



        // 🔧 Маппинг Entity → DTO
        private static EngineerTaskDto MapToDto(TaskEntity task)
        {
            return new EngineerTaskDto
            {
                TaskId = task.WorkOrderId,
                RequestId = task.RequestId,
                ClientFio = task.RequestEntity?.ClientEntity?.User?.FullName ?? "Неизвестно",
                ClientPhone = task.RequestEntity?.ClientEntity?.Phone ?? "",
                SvtType = task.RequestEntity?.SvtType ?? "",
                Model = task.RequestEntity?.Model ?? "",
                SerialNumber = task.RequestEntity?.SerialNumber ?? "",
                Description = task.RequestEntity?.Description ?? "",
                Status = task.Status,
                AssignedAt = task.AssignedAt,
                StartedAt = task.StartedAt,
                CompletedAt = task.CompletedAt,
                WorkName = task.WorkEntity?.WorkName ?? "",
                WorkClassCode = task.WorkEntity?.WorkClassCode ?? 0
            };
        }

    }
}

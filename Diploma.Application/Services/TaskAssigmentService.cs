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
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TaskAssignmentService> _logger;
        private readonly DiplomaDbContext _context;

        public TaskAssignmentService(
            ITaskRepository taskRepository,
            ILogger<TaskAssignmentService> logger, DiplomaDbContext context)
        {
            _taskRepository = taskRepository;
            _logger = logger;
            _context = context;
        }

        public async Task<AssignWorkOrderResponse> AssignWorkOrderAsync(AssignWorkOrderRequest request)
        {
            _logger.LogInformation("========== НАЧАЛО НАЗНАЧЕНИЯ НАРЯДА ==========");
            _logger.LogInformation("📋 Request: WorkId={WorkId}, RequestId={RequestId}, MaxOrders={MaxOrders}",
                request.WorkId, request.RequestId, request.MaxActiveOrders);

            try
            {
                var assignedTask = await _taskRepository.AssignWorkOrderAsync(
                    request.WorkId,
                    request.RequestId,
                    request.MaxActiveOrders ?? 2);

                if (assignedTask == null)
                {
                    _logger.LogWarning("⚠️ Не удалось назначить наряд - возвращен null");
                    return new AssignWorkOrderResponse
                    {
                        Success = false,
                        Message = "Не удалось найти подходящего специалиста"
                    };
                }

                _logger.LogInformation("✅ Наряд успешно назначен: TaskId={TaskId}, EngineerId={EngineerId}",
                    assignedTask.WorkOrderId, assignedTask.ServiceEngineerTabNumber);

                _logger.LogInformation("========== КОНЕЦ НАЗНАЧЕНИЯ ==========");

                return new AssignWorkOrderResponse
                {
                    Success = true,
                    Message = "Наряд успешно назначен",
                    TaskId = assignedTask.WorkOrderId,
                    AssignedEngineerId = assignedTask.ServiceEngineerTabNumber,
                    AssignedAt = assignedTask.AssignedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Критическая ошибка при назначении наряда");
                return new AssignWorkOrderResponse
                {
                    Success = false,
                    Message = $"Ошибка сервера: {ex.Message}"
                };
            }
        }

        public async Task<List<CandidateDto>> GetEligibleCandidatesAsync(int workId, string svtModel)
        {
            _logger.LogInformation("========== ЗАГРУЗКА КАНДИДАТОВ ==========");
            _logger.LogInformation("📋 WorkId={WorkId}, Model={Model}", workId, svtModel);

            try
            {
                var candidates = await _taskRepository.GetEligibleEngineersAsync(workId, svtModel);

                _logger.LogInformation("✅ Найдено кандидатов: {Count}", candidates.Count);

                // Загружаем счётчики активных нарядов для каждого кандидата
                var candidatesWithCounts = new List<(StafEntity Staff, int Count)>();
                foreach (var candidate in candidates)
                {
                    var count = await _taskRepository.GetActiveOrdersCountAsync(candidate.TabNum);
                    candidatesWithCounts.Add((candidate, count));
                }

                var result = candidatesWithCounts.Select(c => new CandidateDto
                {
                    TabNumber = c.Staff.TabNum,
                    // 🔥 Исправлено: загружаем FullName из связанного User
                    FIO = c.Staff.User?.FullName ?? "Неизвестно",
                    Role = c.Staff.Role.ToString(),
                    ActiveOrdersCount = c.Count
                }).ToList();

                _logger.LogInformation("========== КОНЕЦ ЗАГРУЗКИ ==========");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при загрузке кандидатов");
                return new List<CandidateDto>();
            }
        }

        public async Task<bool> StartWorkAsync(int taskId)
        {
            _logger.LogInformation("🚀 Начало работы: TaskId={TaskId}", taskId);
            return true;
        }

        public async Task<bool> CompleteWorkAsync(int taskId)
        {
            _logger.LogInformation("✅ Завершение работы: TaskId={TaskId}", taskId);
            return true;
        }

        public async Task<bool> CancelWorkAsync(int taskId, string reason)
        {
            _logger.LogInformation("❌ Отмена работы: TaskId={TaskId}, Reason={Reason}", taskId, reason);
            return true;
        }

        public async Task<List<TaskEntity>> GetAllTasksAsync()
        {
            _logger.LogInformation("📋 Получение всех нарядов");
            return await _taskRepository.GetAllAsync();
        }

        // ============================================
        // 🔥 СЕРВИС ДЛЯ РЕМОНТА — ИСПРАВЛЕННАЯ ВЕРСИЯ
        // ============================================
        public async Task<AssignRepairWorkOrdersResponse> AssignRepairWorkOrdersAsync(
            AssignRepairWorkOrdersRequest request)
        {
            _logger.LogInformation("========== НАЧАЛО НАЗНАЧЕНИЯ НАРЯДОВ НА РЕМОНТ ==========");
            _logger.LogInformation("📋 Request: RequestId={RequestId}, MaxOrders={MaxOrders}",
                request.RequestId, request.MaxActiveOrders);

            try
            {
                var assignedTasks = await _taskRepository.AssignRepairWorkOrdersAsync(
                    request.RequestId,
                    request.MaxActiveOrders ?? 2);

                if (!assignedTasks.Any())
                {
                    _logger.LogWarning("⚠️ Не удалось назначить наряды на ремонт");
                    return new AssignRepairWorkOrdersResponse
                    {
                        Success = false,
                        Message = "Не удалось найти подходящего специалиста"
                    };
                }

                // 🔥 Исправлено: загружаем сотрудника с навигацией на User
                var engineer = await _context.Stafs
                    .Include(s => s.User)  // 🔥 Ключевая строка!
                    .FirstOrDefaultAsync(s => s.TabNum == assignedTasks.First().ServiceEngineerTabNumber);

                // Формируем список работ с деталями
                var works = new List<RepairWorkItemDto>();
                foreach (var task in assignedTasks)
                {
                    var work = await _context.WorkEntities
                        .FirstOrDefaultAsync(w => w.WorkCode == task.WorkId);

                    works.Add(new RepairWorkItemDto
                    {
                        TaskId = task.WorkOrderId,
                        WorkCode = task.WorkId,
                        WorkName = work?.WorkName ?? $"Работа #{task.WorkId}",
                        EstimatedCost = work?.PricePerHour ?? 0
                    });
                }

                _logger.LogInformation("✅ Назначено {Count} нарядов на ремонт", assignedTasks.Count);
                _logger.LogInformation("========== КОНЕЦ НАЗНАЧЕНИЯ ==========");

                return new AssignRepairWorkOrdersResponse
                {
                    Success = true,
                    Message = $"Назначено {assignedTasks.Count} нарядов на ремонт",
                    TaskIds = assignedTasks.Select(t => t.WorkOrderId).ToList(),
                    AssignedEngineerId = assignedTasks.First().ServiceEngineerTabNumber,
                    // 🔥 Исправлено: берём FullName из engineer.User
                    EngineerName = engineer?.User?.FullName ?? $"Сотрудник #{assignedTasks.First().ServiceEngineerTabNumber}",
                    WorksCount = assignedTasks.Count,
                    Works = works,
                    AssignedAt = assignedTasks.First().AssignedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Критическая ошибка при назначении нарядов на ремонт");
                return new AssignRepairWorkOrdersResponse
                {
                    Success = false,
                    Message = $"Ошибка сервера: {ex.Message}"
                };
            }
        }
    }
}
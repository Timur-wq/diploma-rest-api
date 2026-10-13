using Diploma.Core;
using Diploma.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.DataAccess.Entities.Repositories
{
    public class EngineerTaskRepository : IEngineerTaskRepository
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<EngineerTaskRepository> _logger;
        public EngineerTaskRepository(DiplomaDbContext context, ILogger<EngineerTaskRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<TaskEntity?> CancelTaskAsync(int taskId, string reason)
        {
            _logger.LogInformation("❌ Отмена наряда #{TaskId}, Причина: {Reason}", taskId, reason);

            var task = await _context.TaskEntities.FindAsync(taskId);

            if (task == null)
            {
                _logger.LogError("❌ Наряд #{TaskId} не найден", taskId);
                return null;
            }

            task.Status = "cancelled";
            task.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Наряд #{TaskId} отменён", taskId);
            return task;
        }

        public async Task<TaskEntity?> CompleteTaskAsync(int taskId)
        {
            _logger.LogInformation("✅ Завершение работы по наряду #{TaskId}", taskId);

            var task = await _context.TaskEntities.FindAsync(taskId);

            if (task == null)
            {
                _logger.LogError("❌ Наряд #{TaskId} не найден", taskId);
                return null;
            }

            if (task.Status != "in_progress")
            {
                _logger.LogWarning("⚠️ Наряд #{TaskId} имеет статус {Status}, нельзя завершить", taskId, task.Status);
                return null;
            }

            task.Status = "completed";
            task.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Наряд #{TaskId} завершён", taskId);
            return task;
        }

        public async Task<List<TaskEntity>> GetActiveTasksAsync(int staffTabNumber)
        {
            _logger.LogInformation("📋 Получение активных нарядов для сотрудника #{TabNum}", staffTabNumber);

            return await _context.TaskEntities
                .Include(t => t.RequestEntity)
                    .ThenInclude(r => r.ClientEntity)
                    .ThenInclude(c => c.User)
                .Include(t => t.WorkEntity)
                .Include(t => t.StafEntity)
                .Where(t => t.ServiceEngineerTabNumber == staffTabNumber &&
                           (t.Status == "assigned" || t.Status == "in_progress"))
                .OrderBy(t => t.AssignedAt)
                .ToListAsync();
        }

        public async Task<List<TaskEntity>> GetAllTasksAsync(int staffTabNumber)
        {
            _logger.LogInformation("📋 Получение всех нарядов для сотрудника #{TabNum}", staffTabNumber);

            return await _context.TaskEntities
        .Include(t => t.RequestEntity)           // 🔥 Загружаем заявку
            .ThenInclude(r => r.ClientEntity)    // 🔥 Загружаем клиента
                .ThenInclude(c => c.User)        // 🔥 Загружаем пользователя клиента
        .Include(t => t.WorkEntity)              // 🔥 Загружаем работу
        .Include(t => t.StafEntity)              // 🔥 Загружаем сотрудника
            .ThenInclude(s => s.User)            // 🔥 И его пользователя
        .Where(t => t.ServiceEngineerTabNumber == staffTabNumber)
                .OrderByDescending(t => t.AssignedAt)
                .ToListAsync();
        }

        public async Task<List<TaskEntity>> GetCompletedTasksAsync(int staffTabNumber)
        {
            _logger.LogInformation("📋 Получение завершённых нарядов для сотрудника #{TabNum}", staffTabNumber);

            return await _context.TaskEntities
                .Include(t => t.RequestEntity)
                    .ThenInclude(r => r.ClientEntity)
                    .ThenInclude(c => c.User)
                .Include(t => t.WorkEntity)
                .Include(t => t.StafEntity)
                .Where(t => t.ServiceEngineerTabNumber == staffTabNumber &&
                           t.Status == "completed")
                .OrderByDescending(t => t.CompletedAt)
                .ToListAsync();
        }

        public async Task<Class1.EngineerStatsDto> GetStatsAsync(int staffTabNumber)
        {
            _logger.LogInformation("📊 Получение статистики для сотрудника #{TabNum}", staffTabNumber);

            var allTasks = await _context.TaskEntities
                .Where(t => t.ServiceEngineerTabNumber == staffTabNumber)
                .ToListAsync();

            return new EngineerStatsDto
            {
                TotalTasks = allTasks.Count,
                ActiveTasks = allTasks.Count(t => t.Status == "assigned" || t.Status == "in_progress"),
                CompletedTasks = allTasks.Count(t => t.Status == "completed"),
                CancelledTasks = allTasks.Count(t => t.Status == "cancelled")
            };
        }

        public async Task<TaskEntity?> GetTaskByIdAsync(int taskId, int staffTabNumber)
        {
            _logger.LogInformation("🔍 Поиск наряда #{TaskId} для сотрудника #{TabNum}", taskId, staffTabNumber);

            return await _context.TaskEntities
                .Include(t => t.RequestEntity)
                    .ThenInclude(r => r.ClientEntity)
                    .ThenInclude(c => c.User)
                .Include(t => t.WorkEntity)
                    .ThenInclude(w => w.WorkClass)
                .Include(t => t.StafEntity)
                .FirstOrDefaultAsync(t => t.WorkOrderId == taskId &&
                                         t.ServiceEngineerTabNumber == staffTabNumber);
        }

        public async Task<TaskEntity?> StartTaskAsync(int taskId)
        {
            _logger.LogInformation("🚀 Начало работы по наряду #{TaskId}", taskId);

            var task = await _context.TaskEntities.FindAsync(taskId);

            if (task == null)
            {
                _logger.LogError("❌ Наряд #{TaskId} не найден", taskId);
                return null;
            }

            if (task.Status != "assigned")
            {
                _logger.LogWarning("⚠️ Наряд #{TaskId} имеет статус {Status}, нельзя начать", taskId, task.Status);
                return null;
            }

            task.Status = "in_progress";
            task.StartedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Наряд #{TaskId} начат", taskId);
            return task;
        }
    }
}

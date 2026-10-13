using Diploma.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Core.Abstractions
{
    public interface IEngineerTaskRepository
    {
        Task<List<TaskEntity>> GetAllTasksAsync(int staffTabNumber);
        Task<List<TaskEntity>> GetActiveTasksAsync(int staffTabNumber);
        Task<List<TaskEntity>> GetCompletedTasksAsync(int staffTabNumber);
        Task<TaskEntity?> GetTaskByIdAsync(int taskId, int staffTabNumber);
        Task<EngineerStatsDto> GetStatsAsync(int staffTabNumber);
        Task<TaskEntity?> StartTaskAsync(int taskId);
        Task<TaskEntity?> CompleteTaskAsync(int taskId);
        Task<TaskEntity?> CancelTaskAsync(int taskId, string reason);
    }
}

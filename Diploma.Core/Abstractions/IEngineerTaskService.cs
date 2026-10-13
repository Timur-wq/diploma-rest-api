using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Core.Abstractions
{
    public interface IEngineerTaskService
    {
        Task<List<EngineerTaskDto>> GetAllTasksAsync(int staffTabNumber);
        Task<List<EngineerTaskDto>> GetActiveTasksAsync(int staffTabNumber);
        Task<List<EngineerTaskDto>> GetCompletedTasksAsync(int staffTabNumber);
        Task<EngineerTaskDto?> GetTaskByIdAsync(int taskId, int staffTabNumber);
        Task<EngineerStatsDto> GetStatsAsync(int staffTabNumber);
        Task<bool> StartTaskAsync(int taskId, int staffTabNumber);
        Task<bool> CompleteTaskAsync(int taskId, int staffTabNumber);
        Task<bool> CancelTaskAsync(int taskId, int staffTabNumber, string reason);

        // 🔥 НОВОЕ:
        Task<RepairTaskPdfData?> GetTaskDetailsForPdfAsync(int taskId);
        Task<bool> UpdateTaskDocumentsAsync(int taskId, string actPath, string writeOffPath);
    }
}

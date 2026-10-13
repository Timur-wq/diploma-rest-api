using Diploma.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Abstractions
{
    public interface ITaskRepository
    {
        Task<TaskEntity?> AssignWorkOrderAsync(int workId, int requestId, int maxActiveOrders = 2);
        Task<int> GetActiveOrdersCountAsync(int staffTabNumber);
        Task<List<StafEntity>> GetEligibleEngineersAsync(int workId, string svtModel, int maxActiveOrders = 2);
        Task<TaskEntity?> GetByIdAsync(int taskId);
        Task<List<TaskEntity>> GetAllAsync();
        Task<TaskEntity?> UpdateAsync(TaskEntity task);

        // 🔥 НОВОЕ: Для ремонта (множество работ из акта)
        Task<List<TaskEntity>> AssignRepairWorkOrdersAsync(int requestId, int maxActiveOrders = 2);
    }
}

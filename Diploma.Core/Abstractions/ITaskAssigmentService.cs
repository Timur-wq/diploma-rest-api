using Diploma.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Core.Abstractions
{
    public interface ITaskAssignmentService
    {
        Task<AssignWorkOrderResponse> AssignWorkOrderAsync(AssignWorkOrderRequest request);
        Task<List<CandidateDto>> GetEligibleCandidatesAsync(int workId, string svtModel);
        Task<bool> StartWorkAsync(int taskId);
        Task<bool> CompleteWorkAsync(int taskId);
        Task<bool> CancelWorkAsync(int taskId, string reason);

        Task<List<TaskEntity>> GetAllTasksAsync();

        // 🔥 НОВОЕ: Для ремонта
        Task<AssignRepairWorkOrdersResponse> AssignRepairWorkOrdersAsync(AssignRepairWorkOrdersRequest request);
    }
}

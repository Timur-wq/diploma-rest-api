using Diploma.Core.Models;

namespace Diploma.Core.Abstractions
{
    public interface IRequestService
    {
        Task<int> CreateRequest(RepairRequest repairRequest);
    }
}
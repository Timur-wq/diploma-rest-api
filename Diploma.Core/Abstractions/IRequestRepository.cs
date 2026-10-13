using Diploma.Core.Models;

namespace Diploma.Core.Abstractions
{
    public interface IRequestRepository
    {
        Task<int> CreateRequest(RepairRequest repairRequest);

        
    }
}
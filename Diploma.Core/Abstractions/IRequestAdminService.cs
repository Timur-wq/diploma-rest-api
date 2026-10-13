using Diploma.Core.Enums;
using Diploma.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Abstractions
{
    public interface IRequestAdminService
    {
        Task<List<RequestAdmPanel>> GetAllRequestsAsync();

        //Task<RequestAdmPanel> GetRequestByIdAsync(int id);
        //Task<bool> UpdateRequestStatusAsync(int requestId, UpdateRequestStatus dto, int changedByUserId);
        //Task<int> GetNewRequestsCountAsync();
        //Task<Dictionary<RequestStatus, int>> GetRequestsCountByStatusAsync();
    }

}

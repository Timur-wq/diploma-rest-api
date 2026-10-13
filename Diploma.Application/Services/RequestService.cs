using Diploma.Core.Abstractions;
using Diploma.Core.Models;
using Diploma.DataAccess.Entities.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Application.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _requestRepository;
        public RequestService(IRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task<int> CreateRequest(RepairRequest repairRequest)
        {
            return await _requestRepository.CreateRequest(repairRequest);
        }
    }
}

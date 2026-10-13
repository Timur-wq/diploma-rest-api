using Diploma.Core.Enums;
using Diploma.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Core.Abstractions
{
    public interface IDiagnosticActService
    {
        Task<DiagnosticActResponseDto> CreateActAsync(CreateDiagnosticActRequest request, int engineerTabNumber);
        Task<List<DiagnosticActDto>> GetActsForDispatcherAsync(int? requestId = null, DiagnosticActStatus? status = null);
        Task<DiagnosticActDto?> GetActByIdForDispatcherAsync(int actCode);
        Task<bool> SubmitActToClientAsync(int actCode, int dispatcherTabNum, SubmitActToClientRequest request);
        Task<DiagnosticActDto?> GetActForClientAsync(int actCode, int clientId);
        Task<ActDecisionResponseDto> SubmitClientDecisionAsync(int actCode, int clientId, ClientActDecisionRequest request);
        Task<DiagnosticAct?> GetActByTaskIdAsync(int taskId);
        Task<List<DiagnosticActDto>> GetActsForClientPendingAsync(int clientId);


    }
}

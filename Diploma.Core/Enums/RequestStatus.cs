using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Enums
{
    public enum RequestStatus
    {
        New = 0,
        Accepted = 1,
        Rejected = 2,
        InProgress = 3,
        DiagnosticCompleted = 4,  // 🔥 Новый статус: диагностика завершена, акт отправлен диспетчеру
        WaitingForClientApproval = 5,  // 🔥 Новый статус: ожидание согласования клиента
        ApprovedByClient = 6,  // 🔥 Новый статус: клиент согласовал
        Completed = 7,
        Cancelled = 8
    }
}

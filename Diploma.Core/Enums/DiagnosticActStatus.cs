using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Enums
{
    public enum DiagnosticActStatus
    {
        Draft = 0,              // Черновик (инженер заполняет)
        Submitted = 1,          // Отправлен диспетчеру
        SentToClient = 2,       // Отправлен клиенту на согласование
        ApprovedByClient = 3,   // Клиент согласовал
        RejectedByClient = 4,   // Клиент отклонил
        Completed = 5           // Работы выполнены
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Enums
{
    public enum InvoiceStatus
    {
        Draft = 0,           // Черновик
        Issued = 1,          // Выставлен клиенту
        Paid = 2,            // Оплачен
        Overdue = 3,         // Просрочен
        Cancelled = 4,        // Отменён
        Pending = 5
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Enums
{
    public enum ContractStatus
    {
        Draft = 0,           // Черновик
        Generated = 1,       // Сформирован
        Signed = 2,          // Подписан обеими сторонами
        Cancelled = 3        // Отменён
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Core.Abstractions
{
    public interface IProcurementOptimizationService
    {
        /// <summary>
        /// Запустить жадный алгоритм оптимизации закупок для акта диагностики
        /// </summary>
        Task<ProcurementResultDto> OptimizeProcurementAsync(int actCode);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Core.Abstractions
{
    public interface IStaffService
    {
        Task<StaffDto> CreateAsync(CreateStaffDto dto);
        Task<bool> DeleteAsync(int userId);
        Task<List<StaffDto>> GetAllAsync();
    }
}

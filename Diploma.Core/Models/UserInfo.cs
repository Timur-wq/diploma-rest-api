using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Models
{
    public class UserInfoDto
    {
        public int UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Fio { get; set; } = string.Empty;
        public string? Role { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}

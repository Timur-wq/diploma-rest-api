using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}

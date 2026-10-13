using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Models
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token обязателен")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}

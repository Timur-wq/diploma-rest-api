using Diploma.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Models
{
    public class UpdateRequestStatus
    {
        [Required]
        public RequestStatus Status { get; set; }

        [StringLength(500, ErrorMessage = "Причина не должна превышать 500 символов")]
        public string? RejectionReason { get; set; }

        // Обязательно для статуса Rejected
        public bool IsRejection => Status == RequestStatus.Rejected;
    }
}

using Diploma.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Models
{
    public class RequestAdmPanel
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientFio { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public string? StaffFio { get; set; }
        public RequestStatus Status { get; set; }
        public DateOnly DateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }
        public string SvtType { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }
}

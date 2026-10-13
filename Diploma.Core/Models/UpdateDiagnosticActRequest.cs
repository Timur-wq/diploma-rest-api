using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Models
{
    public class UpdateDiagnosticActRequest
    {
        public string? ExternalCondition { get; set; }
        public string? IdentifiedIssues { get; set; }
        public string? TestResults { get; set; }
        public string? Recommendations { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? EstimatedTime { get; set; }
        public string? DispatcherComment { get; set; }
    }
}

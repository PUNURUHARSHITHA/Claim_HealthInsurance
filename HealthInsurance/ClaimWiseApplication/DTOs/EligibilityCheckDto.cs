using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ClaimWise.Application.DTOs
{
    
        public class EligibilityCheckDto
        {
            public int CheckID { get; set; }
            public int ClaimID { get; set; }
            public string? RuleApplied { get; set; }
            public string? Result { get; set; }
            public string? Reason { get; set; }
            public string? CheckedBy { get; set; }
            public DateTime Timestamp { get; set; }
        }

    
}


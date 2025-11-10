using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class EligibilityResultDto
    {
        public int ClaimID { get; set; }
        public string Result { get; set; } = null!;
        public List<string> RulesApplied { get; set; } = new();
    }

}

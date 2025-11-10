using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Domain.Entities
{
    public class ClaimActionLog
    {
        public int LogID { get; set; }
        public int ClaimID { get; set; }
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public DateTime Timestamp { get; set; }

        // Optional: navigation property
        public Claim Claim { get; set; }
    }

}

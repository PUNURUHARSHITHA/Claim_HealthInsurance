using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Domain.Entities
{
    public class Treatment
    {
        public int TreatmentID { get; set; }
        public string TreatmentName { get; set; }
        public bool IsCovered { get; set; }
        public int WaitingPeriodMonths { get; set; }

        public ICollection<Claim> Claims { get; set; }
    }

}


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClaimWise.Domain.Entities
{
    public class ClaimsReport
    {
        [Key]
        public int ReportID { get; set; }
        public int ClaimID { get; set; }
        public int PolicyholderID { get; set; }
        public string Type { get; set; }
        public DateTime? GeneratedDate { get; set; }
        public string Insights { get; set; }
        public virtual Claim Claim { get; set; }
        public  virtual Policyholder Policyholder { get; set; }
    }

}

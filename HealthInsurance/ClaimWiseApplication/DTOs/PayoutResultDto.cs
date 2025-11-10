using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class PayoutResultDto
    {
        public int ClaimID { get; set; }
        public decimal Amount { get; set; }
        public string ApprovalStatus { get; set; } = "Pending";
        public DateTime? TransferDate { get; set; }
    }

}

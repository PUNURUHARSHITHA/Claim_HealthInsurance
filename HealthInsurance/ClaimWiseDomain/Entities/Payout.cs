using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClaimWise.Domain.Entities
{
    public class Payout
    {
        public int PayoutID { get; set; }
        public int ClaimID { get; set; }
        public decimal? Amount { get; set; }

        public string? ApprovalStatus { get; set; } // "Pending", "Approved", "Rejected"
        public string? PayoutStatus { get; set; }   // "Initiated", "Transferred", "Failed"

        public string? ApprovedByLevel1 { get; set; }
        public string? ApprovedByLevel2 { get; set; }
        public DateTime? Level1ApprovalDate { get; set; }
        public DateTime? Level2ApprovalDate { get; set; }

        public DateTime? TransferDate { get; set; }
        public string? BankReferenceNumber { get; set; }
        public string? TransferStatus { get; set; }

        public virtual Claim Claim { get; set; }
    }


}

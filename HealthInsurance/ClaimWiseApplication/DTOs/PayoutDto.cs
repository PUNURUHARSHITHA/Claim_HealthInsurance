using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClaimWise.Application.DTOs
{
    public class PayoutDto
    {
        public int PayoutID { get; set; }
        public int ClaimID { get; set; }
        public decimal? Amount { get; set; }

        public string? ApprovalStatus { get; set; }
        public string? PayoutStatus { get; set; }

        public string? ApprovedByLevel1 { get; set; }
        public string? ApprovedByLevel2 { get; set; }
        public DateTime? Level1ApprovalDate { get; set; }
        public DateTime? Level2ApprovalDate { get; set; }

        public DateTime? TransferDate { get; set; }
        public string? BankReferenceNumber { get; set; }
        public string? TransferStatus { get; set; }
    }

}


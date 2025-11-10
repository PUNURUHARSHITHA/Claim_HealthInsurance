using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimWise.Domain.Entities
{
    public class Claim
    {
        public int ClaimID { get; set; }

        public int PolicyholderID { get; set; }
        public virtual Policyholder Policyholder { get; set; }

        public int HospitalID { get; set; }
        public virtual Hospital Hospital { get; set; }

        public int TreatmentID { get; set; }
        public virtual Treatment Treatment { get; set; }

        public string? TreatmentDetails { get; set; }
        public string? Documents { get; set; }
        public string? DocumentMetadata { get; set; } // JSON string of file info

        public string? Status { get; set; }
        public DateTime? SubmissionDate { get; set; }

        // Navigation for related entities
        public virtual ICollection<EligibilityCheck> EligibilityChecks { get; set; }
        public virtual ICollection<Payout> Payouts { get; set; }
        public virtual ICollection<ClaimsReport> ClaimsReports { get; set; }
        public ICollection<ClaimActionLog> ClaimActionLogs { get; set; }
        [NotMapped]
        public string? FraudReason { get; set; } // ✅ Used only in memory
       



    }
}

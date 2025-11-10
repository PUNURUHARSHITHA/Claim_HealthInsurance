using System;
using System.Collections.Generic;

namespace ClaimWise.Domain.Entities
{
    public class Policyholder
    {
        public int PolicyholderID { get; set; } // Primary key

        public string Name { get; set; } = string.Empty; // Policyholder's full name

        public int PolicyTypeID { get; set; } // Foreign key to PolicyType (includes PayoutYears)

        public string? CoverageDetails { get; set; } // Optional description of coverage

        public int AgentID { get; set; } // Foreign key to Agent

        public string? Region { get; set; } // Optional region info (e.g., South, North)

        public string? ProductType { get; set; } // Optional product type (e.g., Individual Health)

        public string? PhoneNumber { get; set; } // Optional contact number

        public string? BankReferenceNumber { get; set; } // Optional bank reference for payouts

        public DateTime PolicyStartDate { get; set; } // ✅ Entered by user during policy creation

        public DateTime PolicyEndDate { get; set; } // ✅ Auto-calculated using PolicyType.PayoutYears

        // Navigation properties
        public PolicyType PolicyType { get; set; } // ✅ Includes PolicyDurationYears for end date calculation

        public Agent Agent { get; set; } // Assigned agent for this policyholder

        public ICollection<Claim> Claims { get; set; } = new List<Claim>(); // All claims submitted

        public ICollection<Dependent> Dependents { get; set; } = new List<Dependent>(); // Family members covered

        public ICollection<ClaimsReport> ClaimsReports { get; set; } = new List<ClaimsReport>(); // Audit/report history
    }
}

using System;
using System.Collections.Generic;

namespace ClaimWise.Domain.Entities
{
    public class PolicyType
    {
        public int PolicyTypeID { get; set; } // Primary key for the policy type

        public string TypeName { get; set; } // Name of the policy type (e.g., Health, Life, Vehicle)

        public string Description { get; set; } // Description of what this policy type covers

        public decimal CoverageLimit { get; set; } // Maximum coverage amount allowed under this policy type

        public int PolicyDurationYears { get; set; } // ✅ Duration in years for which the policy remains valid

        // Navigation property: One policy type can be assigned to many policyholders
        public ICollection<Policyholder> Policyholders { get; set; } = new List<Policyholder>();
    }
}

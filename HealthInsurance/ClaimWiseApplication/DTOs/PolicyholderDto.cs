using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClaimWise.Application.DTOs
{
    public class PolicyholderDto : IValidatableObject
    {
        [Key]
        public int PolicyholderID { get; set; }

        //[Required(ErrorMessage = "Name is required")]
        //[MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        //[RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Name must contain only letters and spaces")]
        //[DefaultValue("Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "PolicyTypeID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "PolicyTypeID must be a positive number")]
        [DefaultValue(1)]
        public int PolicyTypeID { get; set; }

        //[MaxLength(500, ErrorMessage = "Coverage details cannot exceed 500 characters")]
        //[DefaultValue("Comprehensive Health Coverage")]
        //[JsonIgnore]
        public string CoverageDetails { get; set; }

        [Required(ErrorMessage = "AgentID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "AgentID must be a positive number")]
        [DefaultValue(1)]
        public int AgentID { get; set; }

        [Required(ErrorMessage = "Region is required")]
        [MaxLength(50)]
        [DefaultValue("Region")]
        public string Region { get; set; }

        [Required(ErrorMessage = "Product type is required")]
        [MaxLength(50)]
        [DefaultValue("Enter Product Type")]
        public string ProductType { get; set; }

        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Phone number must be a valid 10-digit Indian mobile number starting with 6–9")]
        [DefaultValue("9876543210")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Bank reference number is required.")]
        [RegularExpression(@"^[A-Za-z0-9]{12,20}$", ErrorMessage = "Bank reference number must be 12–20 alphanumeric characters.")]
        public string BankReferenceNumber { get; set; }

        [Required]
        public DateTime PolicyStartDate { get; set; }

        // ✅ Added this field to support automatic calculation of PolicyEndDate in controller
        // This is not entered by the user — it's calculated using PolicyStartDate + PolicyType.PolicyDurationYears
        public DateTime PolicyEndDate { get; set; }



        // ✅ Inline validation for Region and ProductType
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var allowedRegions = new HashSet<string>
            {
                "North", "South", "East", "West", "Central", "Northeast", "Zone 1", "Zone 2", "Zone 3"
            };

            var allowedProductTypes = new HashSet<string>
            {
                "Individual Health", "Family Floater", "Senior Citizen", "Maternity Cover",
                "Critical Illness", "Top-Up Plan", "Group Health", "Personal Accident"
            };

            if (!string.IsNullOrWhiteSpace(Region) && !allowedRegions.Contains(Region))
            {
                yield return new ValidationResult(
                    $"Region must be one of: {string.Join(", ", allowedRegions)}",
                    new[] { nameof(Region) });
            }

            if (!string.IsNullOrWhiteSpace(ProductType) && !allowedProductTypes.Contains(ProductType))
            {
                yield return new ValidationResult(
                    $"ProductType must be one of: {string.Join(", ", allowedProductTypes)}",
                    new[] { nameof(ProductType) });
            }
        }
    }
}

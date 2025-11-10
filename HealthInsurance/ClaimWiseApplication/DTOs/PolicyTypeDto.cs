using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ClaimWise.Application.DTOs
{
    public class PolicyTypeDto
    {
        [Key]
        public int PolicyTypeID { get; set; }

        [Required(ErrorMessage = "Type name is required")]
        [MaxLength(50, ErrorMessage = "Type name cannot exceed 50 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Type name must contain only letters and spaces")]
        [DefaultValue("Enter TypeName")]
        public string TypeName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(300, ErrorMessage = "Description cannot exceed 300 characters")]
        [DefaultValue("Enter Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Coverage limit is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Coverage limit must be greater than zero")]
        [DefaultValue(100000.00)]
        public decimal CoverageLimit { get; set; }

        // ✅ Added this field to define how long the policy type is valid
        [Required(ErrorMessage = "Policy duration is required")]
        [Range(1, 100, ErrorMessage = "Policy duration must be between 1 and 100 years")]
        [DefaultValue(1)]
        public int PolicyDurationYears { get; set; }
    }
}

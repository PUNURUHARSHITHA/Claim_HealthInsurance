using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class DependentDto : IValidatableObject
    {
        public int DependentID { get; set; }

        [Required(ErrorMessage = "PolicyholderID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "PolicyholderID must be a positive number")]
        [DefaultValue("Enter Policyholder Id")]
        public int PolicyholderID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Name must contain only letters and spaces")]
        [DefaultValue("Enter Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Relationship is required")]
        [MaxLength(50, ErrorMessage = "Relationship cannot exceed 50 characters")]
        [RegularExpression(@"^(Spouse|Child|Parent|Other)$", ErrorMessage = "Relationship must be one of: Spouse, Child, Parent, Other")]
        [DefaultValue("Enter RelationShip")]
        public string Relationship { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Relationship == "Spouse" && string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Spouse name is required", new[] { nameof(Name) });
            }

            if (Relationship == "Child" && Name.Length < 2)
            {
                yield return new ValidationResult("Child name must be at least 2 characters", new[] { nameof(Name) });
            }
        }
    }
}





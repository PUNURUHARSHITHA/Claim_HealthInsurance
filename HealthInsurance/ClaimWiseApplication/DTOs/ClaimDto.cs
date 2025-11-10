using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class ClaimDto
    {
        [Key]
        public int ClaimID { get; set; }

        [Required(ErrorMessage = "PolicyholderID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "PolicyholderID must be a positive number")]
        public int PolicyholderID { get; set; }

        [Required(ErrorMessage = "HospitalID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "HospitalID must be a positive number")]
        public int HospitalID { get; set; }

        [Required(ErrorMessage = "TreatmentID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "TreatmentID must be a positive number")]
        public int TreatmentID { get; set; }

        [Required(ErrorMessage = "Treatment details are required")]
        public string TreatmentDetails { get; set; }

        [Required(ErrorMessage = "Documents field is required")]
        public string Documents { get; set; }
        public string? DocumentMetadata { get; set; }


        [Required(ErrorMessage = "Status is required")]
        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Submission date is required")]
        public DateTime SubmissionDate { get; set; }
        
    }
}

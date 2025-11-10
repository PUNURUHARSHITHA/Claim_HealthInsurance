using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class TreatmentDto
    {
        [Key]
        public int TreatmentID { get; set; }

        [Required(ErrorMessage = "Treatment name is required")]
        [MaxLength(100)]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Treatment name must contain only letters and spaces")]
        [DefaultValue("Enter Treatment Name")]
        public string TreatmentName { get; set; }

        [Required(ErrorMessage = "Coverage status is required")]
        [DefaultValue(true)]
        public bool IsCovered { get; set; }

        [Range(0, 24, ErrorMessage = "Waiting period must be between 0 and 24 months")]
        [DefaultValue(0)]
        public int WaitingPeriodMonths { get; set; }


       
    }

}




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace ClaimWise.Application.DTOs
{
    public class ClaimsReportDto
    {
        [Key]
        public int ReportID { get; set; }

        [Required(ErrorMessage = "ClaimID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ClaimID must be a positive number")]
        public int ClaimID { get; set; }

        [Required(ErrorMessage = "PolicyholderID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "PolicyholderID must be a positive number")]
        public int PolicyholderID { get; set; }

        [Required(ErrorMessage = "Report type is required")]
        [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Generated date is required")]
        public DateTime? GeneratedDate { get; set; }

        [Required(ErrorMessage = "Insights are required")]
        public string Insights { get; set; }
    }
}

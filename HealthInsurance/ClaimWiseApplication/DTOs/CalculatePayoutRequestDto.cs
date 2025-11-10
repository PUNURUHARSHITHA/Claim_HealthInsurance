using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class CalculatePayoutRequestDto
    {
        [Required(ErrorMessage = "ClaimID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ClaimID must be a positive number")]
        public int ClaimID { get; set; }
    }

}

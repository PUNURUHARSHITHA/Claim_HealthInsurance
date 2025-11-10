using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class UpdateClaimStatusDto
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
        public string UpdatedBy { get;set; }
    }

}

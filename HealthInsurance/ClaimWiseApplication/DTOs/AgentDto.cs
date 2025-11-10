using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;


namespace ClaimWise.Application.DTOs
{
    public class AgentDto
    {
        [DefaultValue(0)]
        public int AgentID { get; set; }

        [Required(ErrorMessage = "Agent name is required")]
        [MaxLength(100, ErrorMessage = "Agent name cannot exceed 100 characters")]
        [DefaultValue("Enter Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Contact number must be a valid 10-digit Indian mobile number starting with 6–9")]
        [DefaultValue("Enter contact number")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email must be a valid Gmail address")]
        [DefaultValue("Enter email")]
        public string Email { get; set; }
    }
}


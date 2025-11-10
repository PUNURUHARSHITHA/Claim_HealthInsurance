using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MinLength(4)]
        [System.ComponentModel.DefaultValue("string")] // 👈 Explicit default for Swagger
        public string Username { get; set; } = "string";

        [Required]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,}$")]
        [System.ComponentModel.DefaultValue("string")]
        public string Password { get; set; } = "string";

        [JsonIgnore]
        public string Role { get; set; } = "string";
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class HospitalDto
    {
        [Key]
        public int HospitalID { get; set; }

        [Required(ErrorMessage = "Hospital name is required")]
        [MaxLength(100)]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Hospital name must contain only letters and spaces")]
        [DefaultValue("Enter Hospital Name")]
        public string HospitalName { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [MaxLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
        [DefaultValue("Enter Location")]
        public string Location { get; set; }


        [Required(ErrorMessage = "Network status is required")]
        [DefaultValue(true)]
        public bool IsNetworkHospital { get; set; }
    }

}


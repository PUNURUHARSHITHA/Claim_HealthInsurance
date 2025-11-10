using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class CreateClaimDto
{
    [Required]
    public int PolicyholderID { get; set; }

    [Required]
    public int HospitalID { get; set; }

    [Required]
    public int TreatmentID { get; set; }

    [Required]
    public string TreatmentDetails { get; set; }

    [Required(ErrorMessage = "At least one document is required")]
    public List<IFormFile> Documents { get; set; }
    // Optional: only if frontend sends metadata
    
}

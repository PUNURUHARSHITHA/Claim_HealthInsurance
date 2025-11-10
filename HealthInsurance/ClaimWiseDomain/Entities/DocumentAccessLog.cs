using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace ClaimWise.Domain.Entities
{
    

    public class DocumentAccessLog
    {
        [Key]
        public int LogID { get; set; }

        [Required]
        public int ClaimID { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccessedBy { get; set; }

        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(500)]
        public string Action { get; set; } // e.g., "Downloaded"
    }

}

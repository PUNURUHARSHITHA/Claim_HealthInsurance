using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class DocumentAccessLogDto
    {
        public int LogID { get; set; }
        public int ClaimID { get; set; }
        public string AccessedBy { get; set; }
        public DateTime AccessedAt { get; set; }
        public string FileName { get; set; }
        public string Action { get; set; }
    }

}

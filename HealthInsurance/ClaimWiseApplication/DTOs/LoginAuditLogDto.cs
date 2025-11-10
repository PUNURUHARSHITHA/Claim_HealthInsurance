using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class LoginAuditLogDto
    {
        public int LogID { get; set; }
        public string Username { get; set; }
        public bool IsSuccessful { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public string Role { get; set; } // from User
        

    }

}

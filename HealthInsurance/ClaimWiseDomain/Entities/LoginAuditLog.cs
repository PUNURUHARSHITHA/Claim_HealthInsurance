using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Domain.Entities
{
    public class LoginAuditLog
    {
        public int LogID { get; set; }
        public int? UserID { get; set; } // Nullable for failed logins
        public string Username { get; set; }
        public bool IsSuccessful { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        


        public User? User { get; set; } // Optional navigation
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Domain.Entities
{

    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        // ✅ Add this navigation property
       
        public ICollection<LoginAuditLog> LoginAuditLogs { get; set; }

    }
}


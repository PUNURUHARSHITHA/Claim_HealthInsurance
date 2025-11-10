using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Domain.Entities
{
    public class RefreshToken
    {
        public int TokenID { get; set; }
        public string Token { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public string? ReplacedByToken { get; set; }

        public User User { get; set; } // Navigation
    }

}

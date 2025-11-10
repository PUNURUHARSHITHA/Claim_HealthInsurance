using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Domain.Entities
{
    public class Agent
    {
        
        public int AgentID { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }

        // Navigation: One agent can have many policyholders
        public virtual ICollection<Policyholder> Policyholders { get; set; }
    }
}

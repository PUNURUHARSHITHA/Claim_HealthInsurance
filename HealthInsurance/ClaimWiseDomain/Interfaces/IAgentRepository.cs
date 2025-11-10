using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Domain.Interfaces
{

    public interface IAgentRepository
    {       
        Task<Agent> GetByIdAsync(int agentId);
        Task<IEnumerable<Agent>> GetAllAsync();
        Task AddAsync(Agent agent);
        Task UpdateAsync(Agent agent);
        Task DeleteAsync(int agentId);
    }

}

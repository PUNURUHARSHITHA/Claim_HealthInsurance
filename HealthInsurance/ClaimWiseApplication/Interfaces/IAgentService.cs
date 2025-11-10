using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;


namespace ClaimWise.Application.Interfaces
{
    

  
        public interface IAgentService
        {
            Task<AgentDto> GetByIdAsync(int agentId);
            Task<IEnumerable<AgentDto>> GetAllAsync();
            Task AddAsync(AgentDto agent);
            Task UpdateAsync(AgentDto agent);
            Task DeleteAsync(int agentId);
        }
    

}

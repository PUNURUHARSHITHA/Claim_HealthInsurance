using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;

namespace ClaimWise.Infrastructure.Repositories
{
    public class AgentRepository : IAgentRepository
    {
        private readonly ClaimWiseDbContext _context;

        public AgentRepository(ClaimWiseDbContext context)
        {
            _context = context;
        }

        public async Task<Agent> GetByIdAsync(int agentId)
        {
            return await _context.Agent.FindAsync(agentId);
        }

        public async Task<IEnumerable<Agent>> GetAllAsync()
        {
            return await _context.Agent.ToListAsync();
        }

        public async Task AddAsync(Agent agent)
        {
            // Ensure EF Core treats this as a new entity and lets SQL Server generate AgentID
            agent.AgentID = 0;
            _context.Entry(agent).Property(a => a.AgentID).IsTemporary = true;

            _context.Agent.Add(agent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Agent agent)
        {
            var existingAgent = await _context.Agent.FindAsync(agent.AgentID);
            if (existingAgent == null)
                throw new KeyNotFoundException($"Agent with ID {agent.AgentID} not found.");

            existingAgent.Name = agent.Name;
            existingAgent.ContactNumber = agent.ContactNumber;
            existingAgent.Email = agent.Email;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int agentId)
        {
            var agent = await _context.Agent.FindAsync(agentId);
            if (agent == null)
                throw new KeyNotFoundException($"Agent with ID {agentId} not found.");

            _context.Agent.Remove(agent);
            await _context.SaveChangesAsync();
        }
    }
}

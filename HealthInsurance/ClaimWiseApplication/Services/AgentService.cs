using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Application.Interfaces;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;


namespace ClaimWise.Application.Services
{
    public class AgentService : IAgentService
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IMapper _mapper;

        public AgentService(IAgentRepository agentRepository, IMapper mapper)
        {
            _agentRepository = agentRepository;
            _mapper = mapper;
        }

        public async Task<AgentDto> GetByIdAsync(int agentId)
        {
            var agent = await _agentRepository.GetByIdAsync(agentId);
            return _mapper.Map<AgentDto>(agent);
        }

        public async Task<IEnumerable<AgentDto>> GetAllAsync()
        {
            var agents = await _agentRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AgentDto>>(agents);
        }

        public async Task AddAsync(AgentDto agentDto)
        {
            var agent = _mapper.Map<Agent>(agentDto);
            await _agentRepository.AddAsync(agent);
        }

        public async Task UpdateAsync(AgentDto agentDto)
        {
            var agent = _mapper.Map<Agent>(agentDto);
            await _agentRepository.UpdateAsync(agent);
        }

        public async Task DeleteAsync(int agentId)
        {
            await _agentRepository.DeleteAsync(agentId);
        }
    }
}

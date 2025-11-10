using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IMapper _mapper;

        public AgentController(IAgentRepository agentRepository, IMapper mapper)
        {
            _agentRepository = agentRepository;
            _mapper = mapper;
        }

        // GET: api/Agent
        [HttpGet]
        [Authorize(Roles = "Admin,Policyholder")]
        [ProducesResponseType(typeof(IEnumerable<AgentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var agents = await _agentRepository.GetAllAsync();
            var agentDtos = _mapper.Map<IEnumerable<AgentDto>>(agents);
            return Ok(agentDtos);
        }

        

        // POST: api/Agent
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] AgentDto agentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var agent = new Agent
            {
                Name = agentDto.Name,
                ContactNumber = agentDto.ContactNumber,
                Email = agentDto.Email
            };

            await _agentRepository.AddAsync(agent);

            var createdAgentDto = _mapper.Map<AgentDto>(agent);
            return CreatedAtAction(nameof(GetAll), new { id = agent.AgentID }, createdAgentDto);
        }

        // PUT: api/Agent/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] AgentDto agentDto)
        {
            if (id != agentDto.AgentID)
                return BadRequest("Agent ID mismatch.");

            var existingAgent = await _agentRepository.GetByIdAsync(id);
            if (existingAgent == null)
                return NotFound($"Agent with ID {id} not found.");

            var agent = _mapper.Map<Agent>(agentDto);
            await _agentRepository.UpdateAsync(agent);
            return NoContent();
        }

        // PATCH: api/Agent/{id}
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonElement updates)
        {
            var agent = await _agentRepository.GetByIdAsync(id);
            if (agent == null)
                return NotFound($"Agent with ID {id} not found.");

            if (updates.TryGetProperty("name", out var nameProp))
                agent.Name = nameProp.GetString();

            if (updates.TryGetProperty("contactNumber", out var contactProp))
                agent.ContactNumber = contactProp.GetString();

            if (updates.TryGetProperty("email", out var emailProp))
                agent.Email = emailProp.GetString();

            await _agentRepository.UpdateAsync(agent);
            return NoContent();
        }

        // DELETE: api/Agent/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var agent = await _agentRepository.GetByIdAsync(id);
            if (agent == null)
                return NotFound($"Agent with ID {id} not found.");

            await _agentRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}

using System.Collections.Generic;
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
    public class PolicyTypeController : ControllerBase
    {
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IMapper _mapper;

        public PolicyTypeController(IPolicyTypeRepository policyTypeRepository, IMapper mapper)
        {
            _policyTypeRepository = policyTypeRepository;
            _mapper = mapper;
        }

        // GET: api/PolicyType
        [HttpGet]
        [Authorize(Roles = "Admin,Policyholder")] // Admin and Policyholder can access
        [ProducesResponseType(typeof(IEnumerable<PolicyTypeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var types = await _policyTypeRepository.GetAllAsync();
            var typeDtos = _mapper.Map<IEnumerable<PolicyTypeDto>>(types);
            return Ok(typeDtos);
        }

        // GET: api/PolicyType/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] // Restrict to Admins
        [ProducesResponseType(typeof(PolicyTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var type = await _policyTypeRepository.GetByIdAsync(id);
            if (type == null)
                return NotFound($"PolicyType with ID {id} not found.");

            var typeDto = _mapper.Map<PolicyTypeDto>(type);
            return Ok(typeDto);
        }

        // POST: api/PolicyType
        [HttpPost]
        [Authorize(Roles = "Admin")] // Restrict to Admins
        [ProducesResponseType(typeof(PolicyTypeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] PolicyTypeDto typeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ Manual mapping updated to include PolicyDurationYears
            var type = new PolicyType
            {
                TypeName = typeDto.TypeName,
                Description = typeDto.Description,
                CoverageLimit = typeDto.CoverageLimit,
                PolicyDurationYears = typeDto.PolicyDurationYears // ✅ Added this line
            };

            await _policyTypeRepository.AddAsync(type);

            var createdDto = _mapper.Map<PolicyTypeDto>(type);
            return CreatedAtAction(nameof(GetById), new { id = type.PolicyTypeID }, createdDto);
        }

        // PUT: api/PolicyType/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Restrict to Admins
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] PolicyTypeDto typeDto)
        {
            if (id != typeDto.PolicyTypeID)
                return BadRequest("PolicyType ID mismatch.");

            var existing = await _policyTypeRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"PolicyType with ID {id} not found.");

            // ✅ Mapping includes PolicyDurationYears via AutoMapper
            var type = _mapper.Map<PolicyType>(typeDto);
            await _policyTypeRepository.UpdateAsync(type);
            return NoContent();
        }

        // DELETE: api/PolicyType/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Restrict to Admins
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _policyTypeRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"PolicyType with ID {id} not found.");

            await _policyTypeRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}

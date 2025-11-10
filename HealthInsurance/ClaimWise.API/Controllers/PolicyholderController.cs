using AutoMapper;

using ClaimWise.Application.DTOs;

using ClaimWise.Domain.Entities;

using ClaimWise.Domain.Interfaces;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

using System.Linq;

using System.Threading.Tasks;

namespace ClaimWise.API.Controllers

{

    [ApiController]

    [Route("api/[controller]")]

    public class PolicyholderController : ControllerBase

    {

        private readonly IPolicyholderRepository _policyholderRepository;

        private readonly IDependentRepository _dependentRepository;

        private readonly IAgentRepository _agentRepository;

        private readonly IPolicyTypeRepository _policyTypeRepository;

        private readonly IMapper _mapper;

        public PolicyholderController(

            IPolicyholderRepository policyholderRepository,

            IDependentRepository dependentRepository,

            IAgentRepository agentRepository,

            IPolicyTypeRepository policyTypeRepository,

            IMapper mapper)

        {

            _policyholderRepository = policyholderRepository;

            _dependentRepository = dependentRepository;

            _agentRepository = agentRepository;

            _policyTypeRepository = policyTypeRepository;

            _mapper = mapper;

        }

        // GET: api/Policyholder

        [HttpGet]

        [Authorize(Roles = "Admin")]

        [ProducesResponseType(typeof(IEnumerable<PolicyholderDto>), StatusCodes.Status200OK)]

        public async Task<IActionResult> GetAll()

        {

            var policyholders = await _policyholderRepository.GetAllAsync();

            var policyholderDtos = _mapper.Map<IEnumerable<PolicyholderDto>>(policyholders);

            return Ok(policyholderDtos);

        }

        // GET: api/Policyholder/self
        [HttpGet("self")]
        [Authorize(Roles = "Policyholder")]
        [ProducesResponseType(typeof(PolicyholderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLoggedInPolicyholder()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized("User name not found in token.");

            var all = await _policyholderRepository.GetAllAsync();
            var match = all.LastOrDefault(p => p.Name.ToLower() == userName.ToLower());

            if (match == null)
                return NotFound("Policyholder record not found.");

            var dto = _mapper.Map<PolicyholderDto>(match);
            return Ok(dto);
        }

        // POST: api/Policyholder

        [HttpPost]

        [Authorize(Roles = "Policyholder")]

        [ProducesResponseType(typeof(PolicyholderDto), StatusCodes.Status201Created)]

        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> Create([FromBody] PolicyholderDto policyholderDto)

        {

            if (!ModelState.IsValid)

                return BadRequest(ModelState);

            // Set name from authenticated user

            var role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            var name = User.Identity?.Name ?? "Unknown";

            if (role == "Policyholder")

            {

                policyholderDto.Name = name;

            }

            // Validate AgentID against existing agents

            var validAgents = await _agentRepository.GetAllAsync();

            var allowedAgentIds = validAgents.Select(a => a.AgentID).ToList();

            if (!allowedAgentIds.Contains(policyholderDto.AgentID))

            {

                return BadRequest($"Invalid Agent ID. Please choose one of the allowed agents: {string.Join(", ", allowedAgentIds)}");

            }

            // Fetch PolicyType to get PolicyDurationYears and CoverageDetails

            var policyType = await _policyTypeRepository.GetByIdAsync(policyholderDto.PolicyTypeID);

            if (policyType == null)

            {

                return BadRequest($"Invalid PolicyTypeID: {policyholderDto.PolicyTypeID}");

            }

            // Auto-fill CoverageDetails if not provided

            if (string.IsNullOrWhiteSpace(policyholderDto.CoverageDetails))

            {

                policyholderDto.CoverageDetails = policyType.Description;

            }

            // Calculate PolicyEndDate using PolicyStartDate + PolicyDurationYears

            var policyEndDate = policyholderDto.PolicyStartDate.AddYears(policyType.PolicyDurationYears);

            // Map DTO to entity

            var policyholder = new Policyholder

            {

                Name = policyholderDto.Name,

                PolicyTypeID = policyholderDto.PolicyTypeID,

                CoverageDetails = policyholderDto.CoverageDetails,

                AgentID = policyholderDto.AgentID,

                Region = policyholderDto.Region,

                ProductType = policyholderDto.ProductType,

                PhoneNumber = policyholderDto.PhoneNumber,

                BankReferenceNumber = policyholderDto.BankReferenceNumber,

                PolicyStartDate = policyholderDto.PolicyStartDate,

                PolicyEndDate = policyEndDate

            };

            // Save to database

            await _policyholderRepository.AddAsync(policyholder);

            // return created DTO

            var createdDto = _mapper.Map<PolicyholderDto>(policyholder);

            return CreatedAtAction(nameof(GetAll), new { id = policyholder.PolicyholderID }, createdDto);

        }

        

    }

}



//// Validate mobile number uniqueness

//var existing = await _policyholderRepository.GetByPhoneNumberAsync(policyholderDto.PhoneNumber);

//if (existing != null)

//{

//    return BadRequest("This mobile number is already registered with another policyholder.");

//}
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Application.Interfaces;
using ClaimWise.Application.Services;
using ClaimWise.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimWise.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EligibilityCheckController : ControllerBase
    {
        private readonly IEligibilityCheckService _eligibilityCheckService;
        private readonly IMapper _mapper;

        public EligibilityCheckController(
            IEligibilityCheckService eligibilityCheckService,
            IMapper mapper)
        {
            _eligibilityCheckService = eligibilityCheckService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("check")]
        public async Task<IActionResult> RunCheck([FromBody] RunEligibilityRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var checkedBy = User.Identity?.Name ?? "Unknown";

            // 🔐 Enforce claim status check before eligibility
            var claim = await _eligibilityCheckService.GetClaimByIdAsync(dto.ClaimID);
            if (claim == null)
                return NotFound($"Claim with ID {dto.ClaimID} not found.");

            if (claim.Status != "UnderReview")
                return BadRequest("Eligibility check can only be run when claim is under review.");

            var result = await _eligibilityCheckService.RunEligibilityCheckAsync(dto.ClaimID, checkedBy);
            return Ok(result);
        }


    }
}

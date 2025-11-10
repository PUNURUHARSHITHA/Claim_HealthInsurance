using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Application.Interfaces;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimWise.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PayoutController : ControllerBase
    {
        private readonly IPayoutRepository _payoutRepository;
        private readonly IPayoutService _payoutService;
        private readonly IClaimActionLogService _logService;
        private readonly IClaimService _claimService;
        private readonly IPolicyholderRepository _policyholderRepository;
        private readonly IMapper _mapper;

        public PayoutController(
            IPayoutRepository payoutRepository,
            IPayoutService payoutService,
            IClaimActionLogService logService,
            IClaimService claimService,
            IPolicyholderRepository policyholderRepository,
            IMapper mapper)
        {
            _payoutRepository = payoutRepository;
            _payoutService = payoutService;
            _logService = logService;
            _claimService = claimService;
            _policyholderRepository = policyholderRepository;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] CalculatePayoutRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var claim = await _claimService.GetClaimByIdAsync(dto.ClaimID);
            if (claim == null)
                return NotFound($"Claim with ID {dto.ClaimID} not found.");
            if (claim.Status == "Rejected")
                return BadRequest("Payout cannot be calculated for a rejected claim.");

            var result = await _payoutService.CalculatePayoutAsync(dto.ClaimID);
            return Ok(result);
        }

       

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payouts = await _payoutRepository.GetAllAsync();
            var payoutDtos = _mapper.Map<IEnumerable<PayoutDto>>(payouts);

            foreach (var dto in payoutDtos)
            {
                if (dto.ApprovedByLevel1 == "Pratheek")
                    dto.ApprovedByLevel1 = "Pratheek (Manager)";
                if (dto.ApprovedByLevel2 == "Lance")
                    dto.ApprovedByLevel2 = "Lance (Admin)";
            }

            return Ok(payoutDtos);
        }


       
        // ✅ Manager-only: Approve payout at Level 1
        [Authorize(Roles = "Manager")]
        [HttpPut("{payoutId}/Manager_Approve-level1")]
        public async Task<IActionResult> ApproveLevel1(int payoutId)
        {
            var username = User.Identity?.Name;
            if (username != "Pratheek")
                return Forbid("Only Pratheek is allowed to approve Level 1 payouts.");

            var payout = await _payoutRepository.GetByIdAsync(payoutId);
            if (payout == null)
                return NotFound($"Payout with ID {payoutId} not found.");

            var claim = await _claimService.GetClaimByIdAsync(payout.ClaimID);
            if (claim == null)
                return NotFound($"Claim with ID {payout.ClaimID} not found.");

            if (claim.Status == "Rejected")
                return BadRequest("Rejected claims cannot be approved for payout.");

            if (!string.IsNullOrEmpty(payout.ApprovedByLevel1) && payout.Level1ApprovalDate.HasValue)
                return BadRequest("Payout already approved at Level 1.");

            await _payoutService.ApproveLevel1Async(payoutId, username);
            await _payoutService.NotifyPolicyholderLevel1ApprovalAsync(payout);

            return Ok(new { message = $"Payment {payoutId} approved at Level 1 by Manager." });

        }




        // Admin-only: Approve payout at Level 2 (after Level 1)

        [Authorize(Roles = "Admin")]

        [HttpPut("{payoutId}/Admin_Approve-level2")]

        public async Task<IActionResult> ApproveLevel2(int payoutId)

        {

            var payout = await _payoutRepository.GetByIdAsync(payoutId);

            if (payout == null)

                return NotFound($"Payout with ID {payoutId} not found.");

            var claim = await _claimService.GetClaimByIdAsync(payout.ClaimID);

            if (claim == null)

                return NotFound($"Claim with ID {payout.ClaimID} not found.");

            if (claim.Status == "Rejected")

                return BadRequest("Rejected claims cannot be approved for payout.");

            if (string.IsNullOrEmpty(payout.ApprovedByLevel1) || !payout.Level1ApprovalDate.HasValue)

                return BadRequest("Level 1 approval must be completed before Level 2.");

            if (!string.IsNullOrEmpty(payout.ApprovedByLevel2) && payout.Level2ApprovalDate.HasValue)

                return BadRequest("Payout already approved at Level 2.");

            var approvedBy = User.Identity?.Name ?? "Unknown";

            await _payoutService.ApproveLevel2Async(payoutId, approvedBy);

            return Ok(new { message = $"Payout {payoutId} approved successfully at Level 2." });


        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{payoutId}/transfer")]
        public async Task<IActionResult> MarkAsTransferred(int payoutId)
        {
            var payout = await _payoutRepository.GetByIdAsync(payoutId);
            if (payout == null)
                return NotFound($"Payout with ID {payoutId} not found.");

            var claim = await _claimService.GetClaimByIdAsync(payout.ClaimID);
            if (claim == null)
                return NotFound($"Claim with ID {payout.ClaimID} not found.");
            if (claim.Status == "Rejected")
                return BadRequest("Rejected claims cannot be transferred.");

            var policyholder = await _policyholderRepository.GetByIdAsync(claim.PolicyholderID);
            if (policyholder == null)
                return NotFound($"Policyholder with ID {claim.PolicyholderID} not found.");
            if (string.IsNullOrWhiteSpace(policyholder.BankReferenceNumber))
                return BadRequest("Bank reference number is missing for this policyholder.");

            var updatedBy = User.Identity?.Name ?? "Unknown";
            await _payoutService.MarkAsTransferredAsync(payoutId, policyholder.BankReferenceNumber, updatedBy);

            // ✅ Notify via service method
            await _payoutService.NotifyPolicyholderPayoutAsync(payout);

            return Ok(new { message = $"Payment {payoutId} marked as transferred and notification sent." });

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("claim/{claimId}/logs")]
        public async Task<IActionResult> GetClaimLogs(int claimId, int page = 1, int size = 10)
        {
            var claim = await _claimService.GetClaimByIdAsync(claimId);
            if (claim == null)
                return NotFound($"Claim with ID {claimId} not found.");
            if (claim.Status == "Rejected")
                return BadRequest("Logs are not available for rejected claims.");

            var logs = await _logService.GetLogsAsync(claimId, page, size);
            return Ok(logs);
        }
       

    }
}

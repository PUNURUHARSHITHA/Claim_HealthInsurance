using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;
using ClaimWise.Application.Helpers;
using ClaimWise.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ClaimWise.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClaimController : ControllerBase
    {
        private readonly IClaimService _claimService;
        private readonly IAuditLogService _auditLogService;
        private readonly IClaimActionLogService _claimActionLogService;
        private readonly IConfiguration _configuration;
        private readonly IClaimRepository _claimRepository;
        private readonly IEligibilityCheckRepository _eligibilityCheckRepository;

        public ClaimController(
            IClaimService claimService,
            IAuditLogService auditLogService,
            IClaimActionLogService claimActionLogService,
            IConfiguration configuration,
            IClaimRepository claimRepository,
            IEligibilityCheckRepository eligibilityCheckRepository)
        {
            _claimService = claimService;
            _auditLogService = auditLogService;
            _claimActionLogService = claimActionLogService;
            _configuration = configuration;
            _claimRepository = claimRepository;
            _eligibilityCheckRepository = eligibilityCheckRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var claims = await _claimService.GetAllClaimsAsync();
            return Ok(claims);
        }

        [Authorize(Roles = "Policyholder")]
        [HttpPost("submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitClaimWithFiles([FromForm] CreateClaimDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingClaim = await _claimService.GetClaimByPolicyholderIdAsync(dto.PolicyholderID);
            if (existingClaim != null)
                return BadRequest("You have already submitted a claim. Only one claim is allowed per policyholder.");

            if (dto.Documents == null || dto.Documents.Count < 1 || dto.Documents.Count > 2)
                return BadRequest("You must upload 1 or 2 documents only.");

            var fileNames = dto.Documents.Select(f => f.FileName.ToLower()).ToList();
            if (fileNames.Distinct().Count() != fileNames.Count)
                return BadRequest("Uploaded documents must have different filenames.");

            foreach (var file in dto.Documents)
            {
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest($"File {file.FileName} exceeds 5 MB limit.");

                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest($"File {file.FileName} has unsupported format. Allowed: PDF, JPG, JPEG, PNG.");

                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                if (fileName.Contains("scan", StringComparison.OrdinalIgnoreCase) ||
                    fileName.Contains("copy", StringComparison.OrdinalIgnoreCase))
                    return BadRequest($"File {file.FileName} appears to be a fraud document (contains 'scan' or 'copy').");
            }

            var claimId = await _claimService.SubmitClaimAsync(dto);
            await _claimActionLogService.LogAsync(claimId, User.Identity?.Name ?? "Unknown", "Claim Submitted");

            return Ok(new { message = $"Claim {claimId} submitted successfully with validated documents." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/Review")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateClaimStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.UpdatedBy = User.Identity?.Name ?? "Unknown";
            dto.Status = "UnderReview";

            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null)
                return NotFound($"Claim with ID {id} not found.");

            if (claim.Status == "Approved")
                return BadRequest("Cannot update status of an already approved claim.");

            var updated = await _claimService.UpdateClaimStatusAsync(id, dto);
            if (!updated)
                return BadRequest("Status update failed. Invalid transition or claim already approved.");

            await _claimActionLogService.LogAsync(id, dto.UpdatedBy, "Claim moved to UnderReview");

            return Ok("Claim status updated to UnderReview.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{claimId}/status")]
        public async Task<IActionResult> ApproveClaim(int claimId)
        {
            var approvedBy = User.Identity?.Name ?? "Unknown";

            var claim = await _claimService.GetClaimByIdAsync(claimId);
            if (claim == null)
                return NotFound($"Claim with ID {claimId} not found.");

            if (claim.Status == "Approved")
                return BadRequest("Claim is already approved.");

            if (claim.Status != "UnderReview")
                return BadRequest("Claim must be under review before it can be approved.");

            var latestCheck = await _eligibilityCheckRepository.GetLatestByClaimIdAsync(claimId);
            if (latestCheck == null)
                return BadRequest("Eligibility check not found. Run eligibility check before approval.");

            if (latestCheck.Result == "Not Eligible")
            {
                await _claimRepository.UpdateStatusAsync(claimId, "Rejected");
                await _claimActionLogService.LogAsync(claimId, approvedBy, "Claim Rejected");
                var rejectedClaim = await _claimRepository.GetByIdAsync(claimId);
                await _claimService.NotifyPolicyholderStatusChangeAsync(rejectedClaim);
                return Ok($"Claim {claimId} rejected based on eligibility check.");
            }

            await _claimRepository.UpdateStatusAsync(claimId, "Approved");
            await _claimActionLogService.LogAsync(claimId, approvedBy, "Claim Approved");
            var approvedClaim = await _claimRepository.GetByIdAsync(claimId);
            await _claimService.NotifyPolicyholderStatusChangeAsync(approvedClaim);
            return Ok($"Claim {claimId} approved successfully.");
        }

        [Authorize(Roles = "Admin,Policyholder")]
        [HttpGet("download/{claimId}")]
        public async Task<IActionResult> DownloadClaimDocument(int claimId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null || string.IsNullOrEmpty(claim.Documents))
                return NotFound("Claim or document not found.");

            var userId = User.Identity?.Name ?? "Unknown";
            var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            if (!string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase) &&
                claim.PolicyholderID.ToString() != userId)
            {
                return Unauthorized("You are not authorized to download this document.");
            }

            var filePath = claim.Documents.Split(';').First();
            var fileName = Path.GetFileName(filePath);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Encrypted file not found.");

            var encryptedBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var encryptionKey = _configuration["Encryption:AESKey"];
            var decryptedBytes = DocumentEncryptionHelper.Decrypt(encryptedBytes, encryptionKey);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();
            var action = $"Downloaded {fileName} from {device} ({ip})";

            await _auditLogService.LogAccessAsync(claim.ClaimID, userId, fileName, action);

            return File(decryptedBytes, "application/octet-stream", fileName);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{claimId}/audit")]
        public async Task<IActionResult> GetAuditLogs(int claimId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var logs = await _auditLogService.GetLogsAsync(claimId, pageNumber, pageSize);
            return Ok(logs);
        }

        [Authorize(Roles = "Admin,Policyholder")]
        [HttpGet("{claimId}/actions")]
        public async Task<IActionResult> GetActionLogs(int claimId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var logs = await _claimActionLogService.GetLogsAsync(claimId, pageNumber, pageSize);
            return Ok(logs);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("status-counts")]
        public async Task<IActionResult> GetClaimStatusCounts()
        {
            var counts = await _claimService.GetClaimStatusCountsAsync();
            return Ok(counts);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("Filtered claims")]
        public async Task<IActionResult> GetClaimsByStatus([FromQuery] string status)
        {
            var claims = await _claimService.GetAllClaimsAsync();
            var filtered = claims.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            return Ok(filtered);
        }

    }
}
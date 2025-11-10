using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Application.Helpers;
using ClaimWise.Application.Interfaces;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ClaimWise.Application.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IMapper _mapper;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IPolicyholderRepository _policyholderRepository;
        private readonly IConfiguration _configuration;
        private readonly IClaimActionLogService _claimActionLogService;

        public ClaimService(
            IClaimRepository claimRepository,
            IPolicyholderRepository policyholderRepository,
            IMapper mapper,
            IWhatsAppService whatsAppService,
            IConfiguration configuration,
            IClaimActionLogService claimActionLogService)
        {
            _claimRepository = claimRepository;
            _mapper = mapper;
            _whatsAppService = whatsAppService;
            _policyholderRepository = policyholderRepository;
            _configuration = configuration;
            _claimActionLogService = claimActionLogService;
        }

        public async Task<int> SubmitClaimAsync(CreateClaimDto dto)
        {
            var savedPaths = new List<string>();
            var metadataList = new List<object>();

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "SecureUploads");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            if (dto.Documents != null && dto.Documents.Any())
            {
                foreach (var file in dto.Documents)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadFolder, fileName);

                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);

                        var encryptionKey = _configuration["Encryption:AESKey"];
                        var encryptedBytes = DocumentEncryptionHelper.Encrypt(memoryStream.ToArray(), encryptionKey);
                        await File.WriteAllBytesAsync(filePath, encryptedBytes);

                        EncryptionAuditLogger.LogEncryption(fileName, dto.PolicyholderID);

                        savedPaths.Add(filePath);

                        metadataList.Add(new
                        {
                            FileName = fileName,
                            SizeKB = file.Length / 1024,
                            Type = file.ContentType
                        });
                    }
                }
            }

            var claim = new Claim
            {
                PolicyholderID = dto.PolicyholderID,
                HospitalID = dto.HospitalID,
                TreatmentID = dto.TreatmentID,
                TreatmentDetails = dto.TreatmentDetails,
                Documents = string.Join(";", savedPaths),
                DocumentMetadata = JsonConvert.SerializeObject(metadataList),
                Status = "Submitted",
                SubmissionDate = DateTime.UtcNow
            };

            await _claimRepository.AddAsync(claim);

            var policyholder = await _policyholderRepository.GetByIdAsync(dto.PolicyholderID);
            if (policyholder != null && !string.IsNullOrEmpty(policyholder.PhoneNumber))
            {
                var formattedNumber = PhoneNumberConverter.ToWhatsAppFormat(policyholder.PhoneNumber);
                if (!string.IsNullOrEmpty(formattedNumber))
                {
                    try
                    {
                        await _whatsAppService.SendMessageAsync(formattedNumber,
                            $"Hi {policyholder.Name}, your claim #{claim.ClaimID} has been submitted successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WhatsApp notification failed: {ex.Message}");
                    }
                }
            }

            return claim.ClaimID;
        }

        public async Task<ClaimDto> GetClaimByIdAsync(int id)
        {
            var claim = await _claimRepository.GetByIdAsync(id);
            return claim == null ? null : _mapper.Map<ClaimDto>(claim);
        }

        public async Task<IEnumerable<ClaimDto>> GetAllClaimsAsync()
        {
            var claims = await _claimRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ClaimDto>>(claims);
        }

        public async Task<bool> UpdateClaimStatusAsync(int claimId, UpdateClaimStatusDto dto)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null)
                return false;

            if (claim.Status == "Approved")
                return false;

            var allowedStatuses = new[] { "UnderReview", "PendingDocuments", "Rejected", "Approved" };
            if (!allowedStatuses.Contains(dto.Status))
                return false;

            // ✅ ENFORCE: Prevent multiple UnderReview transitions
            if (dto.Status == "UnderReview")
            {
                var logs = await _claimActionLogService.GetLogsAsync(claimId, 1, 100); // adjust page/size as needed
                var underReviewCount = logs.Count(log =>
                    log.Action != null && log.Action.ToLower().Contains("underreview"));

                if (underReviewCount > 0)
                    return false; // Already moved to UnderReview once
            }

            claim.Status = dto.Status;
            await _claimRepository.UpdateAsync(claim);

            await _claimActionLogService.LogAsync(claimId, dto.UpdatedBy, $"Status changed to {dto.Status}");

            return true;
        }

        public async Task UpdateClaimAsync(ClaimDto dto)
        {
            var claim = _mapper.Map<Claim>(dto);
            await _claimRepository.UpdateAsync(claim);
        }

        public async Task<bool> DeleteClaimAsync(int id)
        {
            var claim = await _claimRepository.GetByIdAsync(id);
            if (claim == null) return false;

            await _claimRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> ApproveClaimAsync(int claimId, string approvedBy)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null || claim.Status == "Approved")
                return false;

            if (claim.Status != "UnderReview")
                return false;

            claim.Status = "Approved";
            await _claimRepository.UpdateAsync(claim);

            await _claimActionLogService.LogAsync(claimId, approvedBy, "Claim Approved");

            return true;
        }

        public async Task<ClaimDto> GetClaimByPolicyholderIdAsync(int policyholderId)
        {
            var claim = await _claimRepository.GetByPolicyholderIdAsync(policyholderId);
            return claim == null ? null : _mapper.Map<ClaimDto>(claim);
        }

        public async Task<Dictionary<string, int>> GetClaimStatusCountsAsync()
        {
            return await _claimRepository.GetClaimStatusCountsAsync();
        }

        public async Task NotifyPolicyholderStatusChangeAsync(Claim claim)
        {
            var policyholder = await _policyholderRepository.GetByIdAsync(claim.PolicyholderID);
            if (policyholder != null && !string.IsNullOrEmpty(policyholder.PhoneNumber))
            {
                var formattedNumber = PhoneNumberConverter.ToWhatsAppFormat(policyholder.PhoneNumber);
                if (!string.IsNullOrEmpty(formattedNumber))
                {
                    var message = $"Hi {policyholder.Name}, your claim #{claim.ClaimID} has been {claim.Status.ToLower()}.";

                    try
                    {
                        await _whatsAppService.SendMessageAsync(formattedNumber, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WhatsApp notification failed: {ex.Message}");
                    }
                }
            }
        }
    }
}

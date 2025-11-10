using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Application.Helpers;
using ClaimWise.Application.Interfaces;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClaimWise.Application.Services
{
    public class PayoutService : IPayoutService
    {
        private readonly IPayoutRepository _payoutRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimActionLogService _logService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IPolicyholderRepository _policyholderRepository;
        private readonly IMapper _mapper;

        // Constructor injection for all required services and repositories
        public PayoutService(
            IPayoutRepository payoutRepository,
            IClaimRepository claimRepository,
            IClaimActionLogService logService,
            IWhatsAppService whatsAppService,
            IPolicyholderRepository policyholderRepository,
            IMapper mapper)
        {
            _payoutRepository = payoutRepository;
            _claimRepository = claimRepository;
            _logService = logService;
            _whatsAppService = whatsAppService;
            _policyholderRepository = policyholderRepository;
            _mapper = mapper;
        }

        // Calculates payout amount based on policy coverage and creates a payout record
        public async Task<PayoutResultDto> CalculatePayoutAsync(int claimId)
        {
            var claim = await _claimRepository.GetByIdWithPolicyAsync(claimId);

            // Validate claim and policyholder details
            if (claim == null || claim.Policyholder == null || claim.Policyholder.PolicyType == null)
            {
                return new PayoutResultDto
                {
                    ClaimID = claimId,
                    Amount = 0,
                    ApprovalStatus = "Claim not found or missing policy details",
                    TransferDate = null
                };
            }

            // Calculate 80% of coverage limit as payout amount
            var amount = claim.Policyholder.PolicyType.CoverageLimit * 0.8m;

            // Create payout record
            var payout = new Payout
            {
                ClaimID = claimId,
                Amount = amount,
                ApprovalStatus = "Pending",
                PayoutStatus = "Initiated",
                TransferDate = null
            };

            await _payoutRepository.AddAsync(payout);

            // Log payout calculation
            await _logService.LogAsync(claimId, "System", $"Payout calculated: ₹{amount}");

            // Return result DTO
            return new PayoutResultDto
            {
                ClaimID = claimId,
                Amount = amount,
                ApprovalStatus = "Pending",
                TransferDate = null
            };
        }

        // Returns payout details for a given claim
        public async Task<PayoutResultDto?> GetPayoutDetailsAsync(int claimId)
        {
            var payout = await _payoutRepository.GetByClaimIdAsync(claimId);
            if (payout == null) return null;

            return new PayoutResultDto
            {
                ClaimID = payout.ClaimID,
                Amount = payout.Amount ?? 0,
                ApprovalStatus = payout.ApprovalStatus,
                TransferDate = payout.TransferDate
            };
        }

        // Returns full payout DTO for a given claim
        public async Task<PayoutDto> GetByClaimIdAsync(int claimId)
        {
            var payout = await _payoutRepository.GetByClaimIdAsync(claimId);
            return _mapper.Map<PayoutDto>(payout);
        }

        // Adds a new payout manually
        public async Task AddAsync(PayoutDto payoutDto)
        {
            var payout = _mapper.Map<Payout>(payoutDto);
            await _payoutRepository.AddAsync(payout);

            // Log manual creation
            await _logService.LogAsync(payout.ClaimID, "Admin", $"Payout manually created: ₹{payout.Amount}");
        }

        // Updates an existing payout manually
        public async Task UpdateAsync(PayoutDto payoutDto)
        {
            var payout = _mapper.Map<Payout>(payoutDto);
            await _payoutRepository.UpdateAsync(payout);

            // Log manual update
            await _logService.LogAsync(payout.ClaimID, "Admin", $"Payout manually updated");
        }

        // Approves payout at Level 1 and logs the action
        public async Task ApproveLevel1Async(int payoutId, string approvedBy)
        {
            var payout = await _payoutRepository.GetByIdAsync(payoutId);
            if (payout != null)
            {
                payout.ApprovedByLevel1 = approvedBy;
                payout.Level1ApprovalDate = DateTime.UtcNow;
                payout.ApprovalStatus = "Level 1 Approved";

                await _payoutRepository.UpdateAsync(payout);

                // Log Level 1 approval
                await _logService.LogAsync(payout.ClaimID, approvedBy, "Payout Level 1 Approved");
            }
        }

        // Approves payout at Level 2 and logs the action
        public async Task ApproveLevel2Async(int payoutId, string approvedBy)
        {
            var payout = await _payoutRepository.GetByIdAsync(payoutId);
            if (payout != null)
            {
                payout.ApprovedByLevel2 = approvedBy;
                payout.Level2ApprovalDate = DateTime.UtcNow;
                payout.ApprovalStatus = "Level 2 Approved";

                await _payoutRepository.UpdateAsync(payout);

                // Log Level 2 approval
                await _logService.LogAsync(payout.ClaimID, approvedBy, "Payout Level 2 Approved");
            }
        }

        // Marks payout as transferred and logs the action
        public async Task MarkAsTransferredAsync(int payoutId, string bankRef, string updatedBy)
        {
            var payout = await _payoutRepository.GetByIdAsync(payoutId);
            if (payout != null)
            {
                payout.PayoutStatus = "Transferred";
                payout.TransferStatus = "Success";
                payout.TransferDate = DateTime.UtcNow;
                payout.BankReferenceNumber = bankRef;
                payout.ApprovalStatus = "Completed";

                await _payoutRepository.UpdateAsync(payout);

                // Log transfer
                await _logService.LogAsync(payout.ClaimID, updatedBy, $"Payout Transferred (BankRef: {bankRef})");
            }
        }

        // Sends WhatsApp notification to policyholder after payout transfer
        public async Task NotifyPolicyholderPayoutAsync(Payout payout)
        {
            var claim = await _claimRepository.GetByIdWithPolicyAsync(payout.ClaimID);
            var policyholder = claim?.Policyholder;

            if (policyholder != null && !string.IsNullOrWhiteSpace(policyholder.PhoneNumber))
            {
                var formattedNumber = PhoneNumberConverter.ToWhatsAppFormat(policyholder.PhoneNumber);
                if (!string.IsNullOrEmpty(formattedNumber))
                {
                    var message = $"Hi {policyholder.Name}, your payment (ID: {payout.PayoutID}) has been successfully transferred to your account.";

                    try
                    {
                        await _whatsAppService.SendMessageAsync(formattedNumber, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WhatsApp notification failed: {ex.Message}");
                        // Optionally log to file or monitoring system
                    }
                }
            }
        }

        // Sends WhatsApp notification to policyholder after Level 1 approval
        public async Task NotifyPolicyholderLevel1ApprovalAsync(Payout payout)
        {
            var claim = await _claimRepository.GetByIdWithPolicyAsync(payout.ClaimID);
            var policyholder = claim?.Policyholder;

            if (policyholder != null && !string.IsNullOrWhiteSpace(policyholder.PhoneNumber))
            {
                var formattedNumber = PhoneNumberConverter.ToWhatsAppFormat(policyholder.PhoneNumber);
                if (!string.IsNullOrEmpty(formattedNumber))
                {
                    var message = $"Hi {policyholder.Name}, your payout (ID: {payout.PayoutID}) has been approved at Level 1 by Manager Pratheek.";

                    try
                    {
                        await _whatsAppService.SendMessageAsync(formattedNumber, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WhatsApp notification failed: {ex.Message}");
                        // Optionally log to file or monitoring system
                    }
                }
            }
        }
    }
}

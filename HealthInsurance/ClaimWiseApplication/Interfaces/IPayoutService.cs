using System.Threading.Tasks;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Application.Interfaces
{
    public interface IPayoutService
    {
        // Retrieves payout details as DTO for a given claim
        Task<PayoutDto> GetByClaimIdAsync(int claimId);

        // Adds a new payout record manually
        Task AddAsync(PayoutDto payout);

        // Updates an existing payout record manually
        Task UpdateAsync(PayoutDto payout);

        // Calculates payout based on policy coverage and creates payout record
        Task<PayoutResultDto> CalculatePayoutAsync(int claimId);

        // Retrieves payout result details for a given claim
        Task<PayoutResultDto?> GetPayoutDetailsAsync(int claimId);

        // Approves payout at Level 1 and logs the action
        Task ApproveLevel1Async(int payoutId, string approvedBy);

        // Approves payout at Level 2 and logs the action
        Task ApproveLevel2Async(int payoutId, string approvedBy);

        // Marks payout as transferred and updates bank reference info
        Task MarkAsTransferredAsync(int payoutId, string bankRef, string updatedBy);

        // Sends WhatsApp notification to policyholder after payout transfer
        Task NotifyPolicyholderPayoutAsync(Payout payout);

        // ✅ NEW: Sends WhatsApp notification to policyholder after Level 1 approval
        Task NotifyPolicyholderLevel1ApprovalAsync(Payout payout);
    }
}

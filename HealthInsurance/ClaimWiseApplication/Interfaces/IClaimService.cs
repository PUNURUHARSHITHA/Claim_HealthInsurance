using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;


namespace ClaimWise.Application.Interfaces
{
    public interface IClaimService
    {
        Task<int> SubmitClaimAsync(CreateClaimDto dto);

        Task<ClaimDto> GetClaimByIdAsync(int id);
        Task<IEnumerable<ClaimDto>> GetAllClaimsAsync();

        Task UpdateClaimAsync(ClaimDto dto);
        Task<bool> UpdateClaimStatusAsync(int id, UpdateClaimStatusDto dto);
        Task<bool> DeleteClaimAsync(int id);
        Task<bool> ApproveClaimAsync(int claimId, string approvedBy);
        // ✅ ADDED: Used to restrict one claim per policyholder
        Task<ClaimDto> GetClaimByPolicyholderIdAsync(int policyholderId);
        Task<Dictionary<string, int>> GetClaimStatusCountsAsync();
        Task NotifyPolicyholderStatusChangeAsync(Claim claim);
        



    }
}

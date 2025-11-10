using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

public interface IClaimRepository
{
    Task AddAsync(Claim claim);
    Task<Claim> GetByIdAsync(int id);
    Task<IEnumerable<Claim>> GetAllAsync();
    Task UpdateAsync(Claim claim);
    Task DeleteAsync(int id);
    Task UpdateStatusAsync(int id, string status);
    Task<Claim> GetByIdWithPolicyAsync(int claimId);
    Task<IEnumerable<Claim>> GetSuspiciousClaimsAsync();
    // ✅ ADDED: Used to enforce one-claim-per-policyholder rule
    Task<Claim?> GetByPolicyholderIdAsync(int policyholderId);
    Task<Dictionary<string, int>> GetClaimStatusCountsAsync();

    Task<Claim?> GetByIdWithPolicyholderAsync(int claimId);



}

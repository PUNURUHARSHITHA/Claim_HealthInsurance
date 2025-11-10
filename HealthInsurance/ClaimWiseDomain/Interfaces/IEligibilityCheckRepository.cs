using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

public interface IEligibilityCheckRepository
{
    Task<IEnumerable<EligibilityCheck>> GetAllAsync();
    Task<EligibilityCheck> GetByIdAsync(int id);
    Task<IEnumerable<EligibilityCheck>> GetByClaimIdAsync(int claimId); // ✅ NEW
    Task<EligibilityCheck> GetLatestByClaimIdAsync(int claimId);        // ✅ NEW
    Task AddAsync(EligibilityCheck check);
    Task UpdateAsync(EligibilityCheck check);
    Task DeleteAsync(int id);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

public interface IPayoutRepository
{
    Task<IEnumerable<Payout>> GetAllAsync();
    Task<Payout> GetByIdAsync(int id);
    Task<Payout> GetByClaimIdAsync(int claimId); // ✅ NEW
    Task AddAsync(Payout payout);
    Task UpdateAsync(Payout payout);
    Task DeleteAsync(int id);
}

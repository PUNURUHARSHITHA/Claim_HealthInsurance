using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Domain.Interfaces
{
    public interface IClaimActionLogRepository
    {
        // ✅ Add a new audit log entry
        Task AddAsync(ClaimActionLog log);

        // ✅ Get logs for a specific claim with pagination
        Task<IEnumerable<ClaimActionLog>> GetLogsByClaimIdAsync(int claimId, int pageNumber, int pageSize);
    }
}

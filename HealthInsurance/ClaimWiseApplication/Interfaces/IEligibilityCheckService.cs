using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;

namespace ClaimWise.Application.Services
{
    public interface IEligibilityCheckService
    {
        Task<EligibilityCheckDto> GetLatestByClaimIdAsync(int claimId); //  NEW
        Task<IEnumerable<EligibilityCheckDto>> GetAllByClaimIdAsync(int claimId); //  NEW
        Task<ClaimDto> GetClaimByIdAsync(int claimId);

        Task AddAsync(EligibilityCheckDto check); // Manual override
        Task<EligibilityResultDto> RunEligibilityCheckAsync(int claimId, string checkedBy); // With audit
        Task DeleteAsync(int id);

    }
}

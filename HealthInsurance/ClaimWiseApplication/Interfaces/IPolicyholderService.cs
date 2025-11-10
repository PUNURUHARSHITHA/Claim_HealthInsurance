using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;

namespace ClaimWise.Application.Interfaces
{
    public interface IPolicyholderService
    {
        Task<PolicyholderDto> GetByIdAsync(int policyholderId);
        Task<IEnumerable<PolicyholderDto>> GetAllAsync();
        Task AddAsync(PolicyholderDto policyholder);
        Task UpdateAsync(PolicyholderDto policyholder);
        Task DeleteAsync(int policyholderId);

        // ✅ Added to support PolicyEndDate calculation using PolicyDurationYears
        Task<PolicyTypeDto> GetPolicyTypeByIdAsync(int policyTypeId);
    }
}

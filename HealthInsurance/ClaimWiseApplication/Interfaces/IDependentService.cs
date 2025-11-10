using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;

namespace ClaimWise.Application.Interfaces
{
    public interface IDependentService
    {
        Task<DependentDto> GetByIdAsync(int dependentId);
        Task<IEnumerable<DependentDto>> GetAllByPolicyholderIdAsync(int policyholderId);
        Task AddAsync(DependentDto dependent);
        Task UpdateAsync(DependentDto dependent);
        Task DeleteAsync(int dependentId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;


namespace ClaimWise.Application.Interfaces
{
    public interface IPolicyTypeService
    {
        Task<PolicyTypeDto> GetByIdAsync(int policyTypeId);
        Task<IEnumerable<PolicyTypeDto>> GetAllAsync();
        Task AddAsync(PolicyTypeDto policyType);
        Task UpdateAsync(PolicyTypeDto policyType);
        Task DeleteAsync(int policyTypeId);
    }
}


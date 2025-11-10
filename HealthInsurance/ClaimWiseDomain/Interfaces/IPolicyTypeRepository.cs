using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Domain.Interfaces
{
    public interface IPolicyTypeRepository
    {
        Task<IEnumerable<PolicyType>> GetAllAsync();
        Task<PolicyType> GetByIdAsync(int policyhTypeID);
        Task AddAsync(PolicyType policyType);
        Task UpdateAsync(PolicyType policyType);
        Task DeleteAsync(int policyTypeID);
    }


}

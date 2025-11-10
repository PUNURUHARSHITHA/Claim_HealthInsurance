using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;

namespace ClaimWise.Infrastructure.Repositories
{
    public class PolicyTypeRepository : IPolicyTypeRepository
    {
        private readonly ClaimWiseDbContext _context;

        public PolicyTypeRepository(ClaimWiseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PolicyType>> GetAllAsync()
        {
            return await _context.PolicyType.ToListAsync();
        }

        public async Task<PolicyType> GetByIdAsync(int policyTypeId)
        {
            return await _context.PolicyType.FindAsync(policyTypeId);
        }

        public async Task AddAsync(PolicyType policyType)
        {
            await _context.PolicyType.AddAsync(policyType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PolicyType policyType)
        {
            var existing = await _context.PolicyType.FindAsync(policyType.PolicyTypeID);
            if (existing == null)
                throw new KeyNotFoundException($"PolicyType with ID {policyType.PolicyTypeID} not found.");

            existing.TypeName = policyType.TypeName;
            existing.Description = policyType.Description;
            existing.CoverageLimit = policyType.CoverageLimit;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int policyTypeId)
        {
            var policyType = await _context.PolicyType.FindAsync(policyTypeId);
            if (policyType != null)
            {
                _context.PolicyType.Remove(policyType);
                await _context.SaveChangesAsync();
            }
        }
    }
}

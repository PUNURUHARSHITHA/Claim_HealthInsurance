using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;

namespace ClaimWise.Infrastructure.Repositories
{
    public class DependentRepository : IDependentRepository
    {
        private readonly ClaimWiseDbContext _context;

        public DependentRepository(ClaimWiseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Dependent>> GetAllAsync()
        {
            return await _context.Dependent.ToListAsync();
        }

        public async Task<Dependent> GetByIdAsync(int dependentId)
        {
            return await _context.Dependent.FindAsync(dependentId);
        }

        public async Task AddAsync(Dependent dependent)
        {
            await _context.Dependent.AddAsync(dependent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Dependent dependent)
        {
            var existing = await _context.Dependent.FindAsync(dependent.DependentID);
            if (existing == null)
                throw new KeyNotFoundException($"Dependent with ID {dependent.DependentID} not found.");

            existing.Name = dependent.Name;
            existing.Relationship = dependent.Relationship;
            existing.PolicyholderID = dependent.PolicyholderID;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var dependent = await _context.Dependent.FindAsync(id);
            if (dependent != null)
            {
                _context.Dependent.Remove(dependent);
                await _context.SaveChangesAsync();
            }
        }
    }
}

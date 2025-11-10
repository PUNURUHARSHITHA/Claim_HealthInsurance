using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimWise.Infrastructure.Repositories
{
    public class PolicyholderRepository : IPolicyholderRepository
    {
        private readonly ClaimWiseDbContext _context;

        public PolicyholderRepository(ClaimWiseDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ✅ Get a single policyholder by ID with related entities
        public async Task<Policyholder?> GetByIdAsync(int id)
        {
            return await _context.Policyholder
                .Include(p => p.Agent)
                .Include(p => p.PolicyType)
                .Include(p => p.Dependents)
                .FirstOrDefaultAsync(p => p.PolicyholderID == id);
        }

        // ✅ Get all policyholders with related entities
        public async Task<IEnumerable<Policyholder>> GetAllAsync()
        {
            return await _context.Policyholder
                .Include(p => p.Agent)
                .Include(p => p.PolicyType)
                .Include(p => p.Dependents)
                .ToListAsync();
        }

        // ✅ Add a new policyholder
        public async Task AddAsync(Policyholder policyholder)
        {
            if (policyholder == null)
                throw new ArgumentNullException(nameof(policyholder));

            await _context.Policyholder.AddAsync(policyholder);
            await _context.SaveChangesAsync();
        }

        // ✅ Update an existing policyholder
        public async Task UpdateAsync(Policyholder policyholder)
        {
            if (policyholder == null)
                throw new ArgumentNullException(nameof(policyholder));

            var existing = await _context.Policyholder.FindAsync(policyholder.PolicyholderID);
            if (existing == null)
                throw new KeyNotFoundException($"Policyholder with ID {policyholder.PolicyholderID} not found.");

            existing.Name = policyholder.Name;
            existing.CoverageDetails = policyholder.CoverageDetails;
            existing.AgentID = policyholder.AgentID;
            existing.PolicyTypeID = policyholder.PolicyTypeID;
            existing.Region = policyholder.Region;
            existing.ProductType = policyholder.ProductType;
            existing.BankReferenceNumber = policyholder.BankReferenceNumber;

            await _context.SaveChangesAsync();
        }

        // ✅ Delete a policyholder
        public async Task DeleteAsync(int id)
        {
            var policyholder = await _context.Policyholder.FindAsync(id);
            if (policyholder != null)
            {
                _context.Policyholder.Remove(policyholder);
                await _context.SaveChangesAsync();
            }
        }

    }
}

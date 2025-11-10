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
    public class TreatmentRepository : ITreatmentRepository
    {
        private readonly ClaimWiseDbContext _context;

        public TreatmentRepository(ClaimWiseDbContext context)
        {
            _context = context;
        }

        public async Task<Treatment> GetByIdAsync(int treatmentId)
        {
            return await _context.Treatment.FindAsync(treatmentId);
        }

        public async Task<IEnumerable<Treatment>> GetAllAsync()
        {
            return await _context.Treatment.ToListAsync();
        }

        public async Task AddAsync(Treatment treatment)
        {
            _context.Treatment.Add(treatment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Treatment treatment)
        {
            _context.Treatment.Update(treatment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int treatmentId)
        {
            var treatment = await _context.Treatment.FindAsync(treatmentId);
            if (treatment != null)
            {
                _context.Treatment.Remove(treatment);
                await _context.SaveChangesAsync();
            }
        }
    }
}

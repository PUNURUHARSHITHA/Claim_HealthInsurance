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
    public class HospitalRepository : IHospitalRepository
    {
        private readonly ClaimWiseDbContext _context;

        public HospitalRepository(ClaimWiseDbContext context)
        {
            _context = context;
        }

        public async Task<Hospital> GetByIdAsync(int hospitalId)
        {
            return await _context.Hospital.FindAsync(hospitalId);
        }

        public async Task<IEnumerable<Hospital>> GetAllAsync()
        {
            return await _context.Hospital.ToListAsync();
        }

        public async Task AddAsync(Hospital hospital)
        {
            _context.Hospital.Add(hospital);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Hospital hospital)
        {
            _context.Hospital.Update(hospital);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int hospitalId)
        {
            var hospital = await _context.Hospital.FindAsync(hospitalId);
            if (hospital != null)
            {
                _context.Hospital.Remove(hospital);
                await _context.SaveChangesAsync();
            }
        }
    }
}


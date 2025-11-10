using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;

namespace ClaimWise.Infrastructure.Repositories
{
    public class ClaimsReportRepository : IClaimsReportRepository
    {
        private readonly ClaimWiseDbContext _context;

        public ClaimsReportRepository(ClaimWiseDbContext context)
        {
            _context = context;
        }

        //  Get all reports with related claim and policyholder
        public async Task<IEnumerable<ClaimsReport>> GetAllAsync()
        {
            return await _context.ClaimsReport
                .Include(r => r.Claim)
                .Include(r => r.Policyholder)
                .ToListAsync();
        }

        //  Get a single report by ID
        public async Task<ClaimsReport?> GetByIdAsync(int id)
        {
            return await _context.ClaimsReport
                .Include(r => r.Claim)
                .Include(r => r.Policyholder)
                .SingleOrDefaultAsync(r => r.ReportID == id);
        }

        //  Add a new report (used for both manual and generated reports)
        public async Task AddAsync(ClaimsReport report)
        {
            await _context.ClaimsReport.AddAsync(report);
            await _context.SaveChangesAsync();
        }

        //  Update an existing report (e.g., fix insights or type)
        public async Task UpdateAsync(ClaimsReport report)
        {
            _context.ClaimsReport.Update(report);
            await _context.SaveChangesAsync();
        }

        //  Delete a report by ID (used for admin cleanup or test data)
        public async Task DeleteAsync(int id)
        {
            var report = await _context.ClaimsReport.FindAsync(id);
            if (report != null)
            {
                _context.ClaimsReport.Remove(report);
                await _context.SaveChangesAsync();
            }
        }

        //  Filter reports by type (e.g.,   "Eligibility")
        public async Task<IEnumerable<ClaimsReport>> GetByTypeAsync(string type)
        {
            return await _context.ClaimsReport
                .Where(r => r.Type == type)
                .Include(r => r.Claim)
                .Include(r => r.Policyholder)
                .ToListAsync();
        }

        //  Filter reports by date range (used for audit trail and compliance)
        public async Task<IEnumerable<ClaimsReport>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.ClaimsReport
                .Where(r => r.GeneratedDate >= from && r.GeneratedDate <= to)
                .Include(r => r.Claim)
                .Include(r => r.Policyholder)
                .ToListAsync();
        }
    }
}

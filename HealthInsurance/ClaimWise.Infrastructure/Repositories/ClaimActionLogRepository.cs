using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimWise.Infrastructure.Repositories
{
    public class ClaimActionLogRepository : IClaimActionLogRepository
    {
        private readonly ClaimWiseDbContext _context;

        public ClaimActionLogRepository(ClaimWiseDbContext context)
        {
            _context = context;
        }

        // ✅ Add a new audit log entry to the database
        public async Task AddAsync(ClaimActionLog log)
        {
            await _context.ClaimActionLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        // ✅ Retrieve logs for a specific claim, ordered by timestamp (latest first)
        public async Task<IEnumerable<ClaimActionLog>> GetLogsByClaimIdAsync(int claimId, int pageNumber, int pageSize)
        {
            return await _context.ClaimActionLogs
                .Where(l => l.ClaimID == claimId)
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

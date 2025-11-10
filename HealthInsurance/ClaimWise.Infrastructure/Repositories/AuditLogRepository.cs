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
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ClaimWiseDbContext _context;

        public AuditLogRepository(ClaimWiseDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DocumentAccessLog log)
        {
            await _context.DocumentAccessLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DocumentAccessLog>> GetLogsByClaimIdAsync(int claimId, int pageNumber, int pageSize)
        {
            return await _context.DocumentAccessLogs
                .Where(l => l.ClaimID == claimId)
                .OrderByDescending(l => l.AccessedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

    }

}

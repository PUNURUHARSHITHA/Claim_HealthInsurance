using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class EligibilityCheckRepository : IEligibilityCheckRepository
{
    private readonly ClaimWiseDbContext _context;

    public EligibilityCheckRepository(ClaimWiseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EligibilityCheck>> GetAllAsync()
    {
        return await _context.EligibilityCheck.ToListAsync();
    }

    public async Task<EligibilityCheck> GetByIdAsync(int id)
    {
        return await _context.EligibilityCheck.FindAsync(id);
    }

    public async Task<IEnumerable<EligibilityCheck>> GetByClaimIdAsync(int claimId)
    {
        return await _context.EligibilityCheck
            .Where(e => e.ClaimID == claimId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<EligibilityCheck> GetLatestByClaimIdAsync(int claimId)
    {
        return await _context.EligibilityCheck
            .Where(e => e.ClaimID == claimId)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(EligibilityCheck check)
    {
        await _context.EligibilityCheck.AddAsync(check);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(EligibilityCheck check)
    {
        _context.EligibilityCheck.Update(check);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var check = await _context.EligibilityCheck.FindAsync(id);
        if (check != null)
        {
            _context.EligibilityCheck.Remove(check);
            await _context.SaveChangesAsync();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class PayoutRepository : IPayoutRepository
{
    private readonly ClaimWiseDbContext _context;

    public PayoutRepository(ClaimWiseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payout>> GetAllAsync()
    {
        return await _context.Payout.ToListAsync();
    }

    public async Task<Payout> GetByIdAsync(int id)
    {
        return await _context.Payout.FindAsync(id);
    }

    public async Task<Payout> GetByClaimIdAsync(int claimId)
    {
        return await _context.Payout
            .FirstOrDefaultAsync(p => p.ClaimID == claimId);
    }

    public async Task AddAsync(Payout payout)
    {
        await _context.Payout.AddAsync(payout);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payout payout)
    {
        _context.Payout.Update(payout);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var payout = await _context.Payout.FindAsync(id);
        if (payout != null)
        {
            _context.Payout.Remove(payout);
            await _context.SaveChangesAsync();
        }
    }
}

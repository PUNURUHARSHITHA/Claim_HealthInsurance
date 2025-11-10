using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Domain.Interfaces.ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace ClaimWise.Infrastructure.Repositories
{

    namespace ClaimWise.Infrastructure.Repositories
    {
        public class RefreshTokenRepository : IRefreshTokenRepository
        {
            private readonly ClaimWiseDbContext _context;

            public RefreshTokenRepository(ClaimWiseDbContext context)
            {
                _context = context;
            }

            public async Task<RefreshToken?> GetByTokenAsync(string token)
            {
                return await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == token);
            }

            public async Task AddAsync(RefreshToken token)
            {
                _context.RefreshTokens.Add(token);
                await _context.SaveChangesAsync();
            }

            public async Task RevokeAsync(RefreshToken token, string? replacedByToken = null)
            {
                token.IsRevoked = true;
                token.ReplacedByToken = replacedByToken;
                await _context.SaveChangesAsync();
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Application.Interfaces;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Domain.Interfaces.ClaimWise.Domain.Interfaces;

namespace ClaimWise.Application.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _repo;

        public RefreshTokenService(IRefreshTokenRepository repo)
        {
            _repo = repo;
        }

        public async Task<RefreshToken?> ValidateAsync(string token)
        {
            var stored = await _repo.GetByTokenAsync(token);
            if (stored == null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
                return null;

            return stored;
        }

        public async Task<RefreshToken> IssueAsync(User user)
        {
            var token = new RefreshToken
            {
                Token = GenerateSecureToken(),
                UserID = user.UserID,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _repo.AddAsync(token);
            return token;
        }

        public async Task RevokeAsync(RefreshToken token, string? replacedByToken = null)
        {
            await _repo.RevokeAsync(token, replacedByToken);
        }

        private string GenerateSecureToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
        public async Task<bool> RevokeAsync(string token)
        {
            var stored = await _repo.GetByTokenAsync(token);
            if (stored == null || stored.IsRevoked)
                return false;

            await _repo.RevokeAsync(stored);
            return true;
        }

    }
}

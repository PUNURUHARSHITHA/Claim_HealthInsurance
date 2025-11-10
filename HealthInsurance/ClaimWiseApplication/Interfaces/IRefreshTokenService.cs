using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Application.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken?> ValidateAsync(string token);
        Task<RefreshToken> IssueAsync(User user);
        Task RevokeAsync(RefreshToken token, string? replacedByToken = null);
        Task<bool> RevokeAsync(string token);

    }
}


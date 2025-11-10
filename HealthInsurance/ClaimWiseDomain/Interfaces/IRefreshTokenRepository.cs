using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;
namespace ClaimWise.Domain.Interfaces
{

    namespace ClaimWise.Domain.Interfaces
    {
        public interface IRefreshTokenRepository
        {
            Task<RefreshToken?> GetByTokenAsync(string token);
            Task AddAsync(RefreshToken token);
            Task RevokeAsync(RefreshToken token, string? replacedByToken = null);
        }
    }

}

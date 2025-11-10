using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;

namespace ClaimWise.Application.Interfaces
{
    public interface IClaimActionLogService
    {
        Task LogAsync(int claimId, string performedBy, string action);
        Task<IEnumerable<ClaimActionLogDto>> GetLogsAsync(int claimId, int pageNumber, int pageSize);
        
    }

}

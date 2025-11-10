using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ClaimWise.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAccessAsync(int claimId, string accessedBy, string fileName, string action);
        Task<IEnumerable<DocumentAccessLogDto>> GetLogsAsync(int claimId, int pageNumber, int pageSize); // ✅ Fix signature
    }


}

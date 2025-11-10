using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimWise.Domain.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddAsync(DocumentAccessLog log);
        Task<IEnumerable<DocumentAccessLog>> GetLogsByClaimIdAsync(int claimId, int pageNumber, int pageSize); // ✅ Fix signature
    }


}

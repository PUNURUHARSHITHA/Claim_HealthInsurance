using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Application.Interfaces;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClaimWise.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repo;
        private readonly IMapper _mapper;

        public AuditLogService(IAuditLogRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task LogAccessAsync(int claimId, string accessedBy, string fileName, string action)
        {
            var log = new DocumentAccessLog
            {
                ClaimID = claimId,
                AccessedBy = accessedBy,
                FileName = fileName,
                Action = action
            };

            await _repo.AddAsync(log);
        }

        public async Task<IEnumerable<DocumentAccessLogDto>> GetLogsAsync(int claimId, int pageNumber, int pageSize)
        {
            var logs = await _repo.GetLogsByClaimIdAsync(claimId, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<DocumentAccessLogDto>>(logs);
        }

    }

}

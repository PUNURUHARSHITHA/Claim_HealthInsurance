using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Application.Interfaces;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;

namespace ClaimWise.Application.Services
{
    public class ClaimActionLogService : IClaimActionLogService
    {
        private readonly IClaimActionLogRepository _repo;
        private readonly IMapper _mapper;

        public ClaimActionLogService(IClaimActionLogRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // ✅ Log any action (status change, approval, rejection, etc.)
        public async Task LogAsync(int claimId, string performedBy, string action)
        {
            var log = new ClaimActionLog
            {
                ClaimID = claimId,
                PerformedBy = performedBy,
                Action = action,
                Timestamp = DateTime.UtcNow
            };

            await _repo.AddAsync(log);
        }

        // ✅ Get logs for a claim with pagination
        public async Task<IEnumerable<ClaimActionLogDto>> GetLogsAsync(int claimId, int pageNumber, int pageSize)
        {
            var logs = await _repo.GetLogsByClaimIdAsync(claimId, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<ClaimActionLogDto>>(logs);
        }
    }
}

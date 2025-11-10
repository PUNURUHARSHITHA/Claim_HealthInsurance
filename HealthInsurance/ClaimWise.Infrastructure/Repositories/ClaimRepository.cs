using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;

namespace ClaimWise.Infrastructure.Repositories
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly ClaimWiseDbContext _context;

        public ClaimRepository(ClaimWiseDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ✅ Get all claims with related entities
        public async Task<IEnumerable<Claim>> GetAllAsync()
        {
            return await _context.Claim
                .Include(c => c.Policyholder)
                .Include(c => c.Hospital)
                .Include(c => c.Treatment)
                .ToListAsync();
        }

        // ✅ Get a single claim by ID with related entities
        public async Task<Claim?> GetByIdAsync(int id)
        {
            return await _context.Claim
                .Include(c => c.Policyholder)
                .Include(c => c.Hospital)
                .Include(c => c.Treatment)
                .FirstOrDefaultAsync(c => c.ClaimID == id);
        }

        // ✅ NEW: Get claim by ID including Policyholder only (for eligibility check)
        public async Task<Claim?> GetByIdWithPolicyholderAsync(int claimId)
        {
            return await _context.Claim
                .Include(c => c.Policyholder)
                .FirstOrDefaultAsync(c => c.ClaimID == claimId);
        }

        // ✅ Add a new claim
        public async Task AddAsync(Claim claim)
        {
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            await _context.Claim.AddAsync(claim);
            await _context.SaveChangesAsync();
        }

        // ✅ Update an existing claim
        public async Task UpdateAsync(Claim claim)
        {
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            _context.Claim.Update(claim);
            await _context.SaveChangesAsync();
        }

        // ✅ Update only the status of a claim
        public async Task UpdateStatusAsync(int id, string status)
        {
            var claim = await _context.Claim.FindAsync(id);
            if (claim != null)
            {
                claim.Status = status;
                _context.Claim.Update(claim);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Delete a claim and all related records
        public async Task DeleteAsync(int id)
        {
            var relatedChecks = await _context.EligibilityCheck
                .Where(e => e.ClaimID == id)
                .ToListAsync();

            if (relatedChecks.Any())
                _context.EligibilityCheck.RemoveRange(relatedChecks);

            var relatedPayouts = await _context.Payout
                .Where(p => p.ClaimID == id)
                .ToListAsync();

            if (relatedPayouts.Any())
                _context.Payout.RemoveRange(relatedPayouts);

            var relatedReports = await _context.ClaimsReport
                .Where(r => r.ClaimID == id)
                .ToListAsync();

            if (relatedReports.Any())
                _context.ClaimsReport.RemoveRange(relatedReports);

            var claim = await _context.Claim.FindAsync(id);
            if (claim != null)
                _context.Claim.Remove(claim);

            await _context.SaveChangesAsync();
        }

        // ✅ Detect claims with suspicious document metadata
        public async Task<IEnumerable<Claim>> GetClaimsWithSuspiciousDocumentsAsync()
        {
            return await _context.Claim
                .Where(c => c.DocumentMetadata != null &&
                            (c.DocumentMetadata.Contains("scan") || c.DocumentMetadata.Contains("small")))
                .ToListAsync();
        }

        // ✅ Get claim with policyholder and policy type
        public async Task<Claim?> GetByIdWithPolicyAsync(int claimId)
        {
            return await _context.Claim
                .Include(c => c.Policyholder)
                    .ThenInclude(ph => ph.PolicyType)
                .Include(c => c.Hospital)
                .Include(c => c.Treatment)
                .FirstOrDefaultAsync(c => c.ClaimID == claimId);
        }

        // ✅ Detect suspicious claims for fraud reporting
        public async Task<IEnumerable<Claim>> GetSuspiciousClaimsAsync()
        {
            var suspiciousClaims = await _context.Claim
                .Include(c => c.Policyholder)
                .Include(c => c.Hospital)
                .Include(c => c.Treatment)
                .Where(c =>
                    c.SubmissionDate >= DateTime.UtcNow.AddDays(-30) &&
                    (
                        (c.Documents != null && (c.Documents.Contains("scan") || c.Documents.Contains("copy"))) ||
                        (c.Hospital != null && !c.Hospital.IsNetworkHospital) ||
                        (c.Treatment != null && !c.Treatment.IsCovered)
                    )
                )
                .ToListAsync();

            foreach (var claim in suspiciousClaims)
            {
                var reasons = new List<string>();

                if (claim.Documents != null && (claim.Documents.Contains("scan") || claim.Documents.Contains("copy")))
                    reasons.Add("suspicious document content");

                if (claim.Hospital != null && !claim.Hospital.IsNetworkHospital)
                    reasons.Add("non-network hospital");

                if (claim.Treatment != null && !claim.Treatment.IsCovered)
                    reasons.Add("treatment not covered");

                claim.FraudReason = string.Join(", ", reasons);
            }

            return suspiciousClaims;
        }

        // ✅ Get claim by policyholder ID to enforce one-claim-per-policyholder rule
        public async Task<Claim?> GetByPolicyholderIdAsync(int policyholderId)
        {
            return await _context.Claim
                .Include(c => c.Policyholder)
                .Include(c => c.Hospital)
                .Include(c => c.Treatment)
                .FirstOrDefaultAsync(c => c.PolicyholderID == policyholderId);
        }

        // ✅ Get claim status counts for dashboard analytics
        public async Task<Dictionary<string, int>> GetClaimStatusCountsAsync()
        {
            return await _context.Claim
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);
        }
    }
}

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

namespace ClaimWise.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IClaimsReportRepository _claimsReportRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IEligibilityCheckRepository _eligibilityCheckRepository;
        private readonly IMapper _mapper;

        public ReportService(
            IClaimsReportRepository claimsReportRepository,
            IClaimRepository claimRepository,
            IEligibilityCheckRepository eligibilityCheckRepository,
            IMapper mapper)
        {
            _claimsReportRepository = claimsReportRepository;
            _claimRepository = claimRepository;
            _eligibilityCheckRepository = eligibilityCheckRepository;
            _mapper = mapper;
        }

        public async Task<ClaimsReportDto> GetByIdAsync(int reportId)
        {
            var report = await _claimsReportRepository.GetByIdAsync(reportId);
            return _mapper.Map<ClaimsReportDto>(report);
        }

        public async Task<IEnumerable<ClaimsReportDto>> GetAllAsync()
        {
            var reports = await _claimsReportRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ClaimsReportDto>>(reports);
        }

        public async Task AddAsync(ClaimsReportDto reportDto)
        {
            var report = _mapper.Map<ClaimsReport>(reportDto);
            report.GeneratedDate ??= DateTime.UtcNow;
            report.Type ??= "Manual";
            report.Insights ??= $"Manual report created for Claim {report.ClaimID}";
            await _claimsReportRepository.AddAsync(report);
        }

        public async Task<ClaimsReportDto> GenerateFraudReportAsync(int claimId)
        {
            var claim = await _claimRepository.GetByIdWithPolicyAsync(claimId);
            if (claim == null)
                throw new ArgumentException($"Claim with ID {claimId} not found.");

            var latestCheck = await _eligibilityCheckRepository.GetLatestByClaimIdAsync(claimId);

            string type;
            string insights;

            if (!string.IsNullOrEmpty(claim.FraudReason))
            {
                type = "Fraud";
                insights = $"Claim {claim.ClaimID} flagged due to {claim.FraudReason}";
            }
            else if (claim.Treatment != null && !claim.Treatment.IsCovered)
            {
                type = "CoverageMismatch";
                insights = $"Claim {claim.ClaimID} involves uncovered treatment: {claim.Treatment.TreatmentName}";
            }
            else if (claim.Hospital != null && !claim.Hospital.IsNetworkHospital)
            {
                type = "NetworkViolation";
                insights = $"Claim {claim.ClaimID} submitted from non-network hospital: {claim.Hospital.HospitalName}";
            }
            else if (claim.Payouts != null && claim.Payouts.Any(p => p.ApprovalStatus == "Rejected"))
            {
                type = "PayoutRejected";
                insights = $"Claim {claim.ClaimID} payout was rejected";
            }
            else if (latestCheck != null && latestCheck.Result == "Not Eligible")
            {
                type = "EligibilityFailure";
                insights = $"Claim {claim.ClaimID} failed eligibility check: {latestCheck.RuleApplied}";
            }
            else if (claim.Status == "Approved")
            {
                type = "Eligible";
                insights = $"Claim {claim.ClaimID} passed eligibility checks and was approved";
            }
            else
            {
                type = "Turnaround";
                var days = DateTime.UtcNow.Subtract(claim.SubmissionDate ?? DateTime.UtcNow).Days;
                insights = $"Claim {claim.ClaimID} processed in {days} days";
            }

            var report = new ClaimsReport
            {
                ClaimID = claim.ClaimID,
                PolicyholderID = claim.PolicyholderID,
                Type = type,
                GeneratedDate = DateTime.UtcNow,
                Insights = insights
            };

            await _claimsReportRepository.AddAsync(report);
            return _mapper.Map<ClaimsReportDto>(report);
        }

      
        


    }
}

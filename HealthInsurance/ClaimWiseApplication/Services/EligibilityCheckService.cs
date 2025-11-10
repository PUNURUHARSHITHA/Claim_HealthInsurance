using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Application.Interfaces;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;

namespace ClaimWise.Application.Services
{
    public class EligibilityCheckService : IEligibilityCheckService
    {
        private readonly IEligibilityCheckRepository _eligibilityCheckRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IMapper _mapper;

        public EligibilityCheckService(
            IEligibilityCheckRepository eligibilityCheckRepository,
            IClaimRepository claimRepository,
            IMapper mapper)
        {
            _eligibilityCheckRepository = eligibilityCheckRepository;
            _claimRepository = claimRepository;
            _mapper = mapper;
        }

        public async Task<EligibilityResultDto> RunEligibilityCheckAsync(int claimId, string checkedBy)
        {
            var claim = await _claimRepository.GetByIdWithPolicyholderAsync(claimId);
            var rules = new List<string>();
            var result = "Eligible";
            var reason = "All rules passed";

            if (claim == null)
            {
                result = "Claim not found";
                reason = "Claim does not exist";
                rules.Add(reason);
            }
            else
            {
                if (!claim.Treatment.IsCovered)
                {
                    rules.Add("Treatment not covered");
                    result = "Not Eligible";
                }

                if (!claim.Hospital.IsNetworkHospital)
                {
                    rules.Add("Hospital not in network");
                    result = "Not Eligible";
                }

                if (claim.SubmissionDate < claim.Policyholder.PolicyStartDate)
                {
                    rules.Add("Policy not active on claim date");
                    result = "Not Eligible";
                }

                var waitingMonths = claim.Treatment.WaitingPeriodMonths;
                var eligibleDate = claim.Policyholder.PolicyStartDate.AddMonths(waitingMonths);

                if (claim.SubmissionDate < eligibleDate)
                {
                    rules.Add($"Waiting period not completed ({waitingMonths} months)");
                    result = "Not Eligible";
                }

                var similarClaims = await _claimRepository.GetAllAsync();
                var duplicate = similarClaims.Any(c =>
                    c.ClaimID != claim.ClaimID &&
                    c.PolicyholderID == claim.PolicyholderID &&
                    c.TreatmentID == claim.TreatmentID &&
                    c.HospitalID == claim.HospitalID);
                if (duplicate)
                {
                    rules.Add("Duplicate claim detected for same treatment and hospital");
                    result = "Not Eligible";
                }

                if (string.IsNullOrWhiteSpace(claim.Documents))
                {
                    rules.Add("No documents submitted");
                    result = "Not Eligible";
                }

                if (claim.Status == "Approved")
                {
                    rules.Add("Claim already approved");
                    result = "Not Eligible";
                }

                if (claim.Status != "UnderReview")
                {
                    rules.Add("Claim must be under review before eligibility check");
                    result = "Not Eligible";
                }

                if (result == "Eligible")
                {
                    rules.Add("All rules passed");
                }

                reason = result == "Eligible" ? "All rules passed" : string.Join("; ", rules);
            }

            var check = new EligibilityCheck
            {
                ClaimID = claimId,
                RuleApplied = string.Join("; ", rules),
                Result = result,
                Reason = reason,
                CheckedBy = checkedBy,
                Timestamp = DateTime.UtcNow
            };

            await _eligibilityCheckRepository.AddAsync(check);

            return new EligibilityResultDto
            {
                ClaimID = claimId,
                Result = result,
                RulesApplied = rules
            };
        }

        public async Task<EligibilityCheckDto> GetLatestByClaimIdAsync(int claimId)
        {
            var check = await _eligibilityCheckRepository.GetLatestByClaimIdAsync(claimId);
            return _mapper.Map<EligibilityCheckDto>(check);
        }

        public async Task<IEnumerable<EligibilityCheckDto>> GetAllByClaimIdAsync(int claimId)
        {
            var checks = await _eligibilityCheckRepository.GetByClaimIdAsync(claimId);
            return _mapper.Map<IEnumerable<EligibilityCheckDto>>(checks);
        }

        public async Task AddAsync(EligibilityCheckDto checkDto)
        {
            var check = _mapper.Map<EligibilityCheck>(checkDto);
            check.Timestamp = DateTime.UtcNow;
            await _eligibilityCheckRepository.AddAsync(check);
        }

        public async Task DeleteAsync(int id)
        {
            var check = await _eligibilityCheckRepository.GetByIdAsync(id);
            if (check != null)
            {
                await _eligibilityCheckRepository.DeleteAsync(id);
            }
        }

        public async Task<ClaimDto> GetClaimByIdAsync(int claimId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            return _mapper.Map<ClaimDto>(claim);
        }
    }
}

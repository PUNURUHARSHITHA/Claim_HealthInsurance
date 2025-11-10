

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Application.Interfaces;

namespace ClaimWise.Application.Services
{
    public class PolicyholderService : IPolicyholderService
    {
        private readonly IPolicyholderRepository _policyholderRepository;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IMapper _mapper;

        public PolicyholderService(
            IPolicyholderRepository policyholderRepository,
            IPolicyTypeRepository policyTypeRepository,
            IMapper mapper)
        {
            _policyholderRepository = policyholderRepository;
            _policyTypeRepository = policyTypeRepository;
            _mapper = mapper;
        }

        // ✅ Fetch a single policyholder by ID
        public async Task<PolicyholderDto> GetByIdAsync(int policyholderId)
        {
            var policyholder = await _policyholderRepository.GetByIdAsync(policyholderId);
            return _mapper.Map<PolicyholderDto>(policyholder);
        }

        // ✅ Fetch all policyholders
        public async Task<IEnumerable<PolicyholderDto>> GetAllAsync()
        {
            var policyholders = await _policyholderRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PolicyholderDto>>(policyholders);
        }

        // ✅ Add a new policyholder with correct duration logic
        public async Task AddAsync(PolicyholderDto policyholderDto)
        {
            var policyType = await _policyTypeRepository.GetByIdAsync(policyholderDto.PolicyTypeID);

            if (policyType != null)
            {
                // ✅ Auto-fill coverage if missing
                if (string.IsNullOrWhiteSpace(policyholderDto.CoverageDetails))
                {
                    policyholderDto.CoverageDetails = policyType.Description;
                }

                // ✅ Recalculate end date using correct duration
                policyholderDto.PolicyEndDate = policyholderDto.PolicyStartDate.AddYears(policyType.PolicyDurationYears);
            }
            else
            {
                // ✅ Fallback if policy type is invalid
                policyholderDto.CoverageDetails = "Unknown coverage type";
                policyholderDto.PolicyEndDate = policyholderDto.PolicyStartDate.AddYears(1); // Default to 1 year
            }

            var policyholder = _mapper.Map<Policyholder>(policyholderDto);
            await _policyholderRepository.AddAsync(policyholder);
        }

        // ✅ Update existing policyholder
        public async Task UpdateAsync(PolicyholderDto policyholderDto)
        {
            var policyType = await _policyTypeRepository.GetByIdAsync(policyholderDto.PolicyTypeID);

            if (policyType != null)
            {
                policyholderDto.CoverageDetails = policyType.Description;
                policyholderDto.PolicyEndDate = policyholderDto.PolicyStartDate.AddYears(policyType.PolicyDurationYears);
            }

            var policyholder = _mapper.Map<Policyholder>(policyholderDto);
            await _policyholderRepository.UpdateAsync(policyholder);
        }

        // ✅ Delete by ID
        public async Task DeleteAsync(int policyholderId)
        {
            await _policyholderRepository.DeleteAsync(policyholderId);
        }

        // ✅ Utility: Fetch policy type as DTO
        public async Task<PolicyTypeDto> GetPolicyTypeByIdAsync(int policyTypeId)
        {
            var type = await _policyTypeRepository.GetByIdAsync(policyTypeId);
            return _mapper.Map<PolicyTypeDto>(type);
        }
    }
}

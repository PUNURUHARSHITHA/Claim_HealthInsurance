using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Application.Interfaces;

namespace ClaimWise.Application.Services
{
    public class PolicyTypeService : IPolicyTypeService
    {
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IMapper _mapper;

        public PolicyTypeService(IPolicyTypeRepository policyTypeRepository, IMapper mapper)
        {
            _policyTypeRepository = policyTypeRepository;
            _mapper = mapper;
        }

        public async Task<PolicyTypeDto> GetByIdAsync(int policyTypeId)
        {
            var policyType = await _policyTypeRepository.GetByIdAsync(policyTypeId);
            return _mapper.Map<PolicyTypeDto>(policyType);
        }

        public async Task<IEnumerable<PolicyTypeDto>> GetAllAsync()
        {
            var policyTypes = await _policyTypeRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PolicyTypeDto>>(policyTypes);
        }

        public async Task AddAsync(PolicyTypeDto policyTypeDto)
        {
            var policyType = _mapper.Map<PolicyType>(policyTypeDto);
            await _policyTypeRepository.AddAsync(policyType);
        }

        public async Task UpdateAsync(PolicyTypeDto policyTypeDto)
        {
            var policyType = _mapper.Map<PolicyType>(policyTypeDto);
            await _policyTypeRepository.UpdateAsync(policyType);
        }

        public async Task DeleteAsync(int policyTypeId)
        {
            await _policyTypeRepository.DeleteAsync(policyTypeId);
        }
    }
}

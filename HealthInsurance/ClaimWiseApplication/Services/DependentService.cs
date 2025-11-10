using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Application.Interfaces;

namespace ClaimWise.Application.Services
{
    public class DependentService : IDependentService
    {
        private readonly IDependentRepository _dependentRepository;
        private readonly IMapper _mapper;

        public DependentService(IDependentRepository dependentRepository, IMapper mapper)
        {
            _dependentRepository = dependentRepository;
            _mapper = mapper;
        }

        public async Task<DependentDto> GetByIdAsync(int dependentId)
        {
            var dependent = await _dependentRepository.GetByIdAsync(dependentId);
            return _mapper.Map<DependentDto>(dependent);
        }

        public async Task<IEnumerable<DependentDto>> GetAllByPolicyholderIdAsync(int policyholderId)
        {
            var allDependents = await _dependentRepository.GetAllAsync();
            var filteredDependents = allDependents.Where(d => d.PolicyholderID == policyholderId);
            return _mapper.Map<IEnumerable<DependentDto>>(filteredDependents);
        }

        public async Task AddAsync(DependentDto dependentDto)
        {
            var dependent = _mapper.Map<Dependent>(dependentDto);
            await _dependentRepository.AddAsync(dependent);
        }

        public async Task UpdateAsync(DependentDto dependentDto)
        {
            var dependent = _mapper.Map<Dependent>(dependentDto);
            await _dependentRepository.UpdateAsync(dependent);
        }

        public async Task DeleteAsync(int dependentId)
        {
            await _dependentRepository.DeleteAsync(dependentId);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Application.Interfaces;

namespace ClaimWise.Application.Services
{
    public class HospitalService : IHospitalService
    {
        private readonly IHospitalRepository _hospitalRepository;
        private readonly IMapper _mapper;

        public HospitalService(IHospitalRepository hospitalRepository, IMapper mapper)
        {
            _hospitalRepository = hospitalRepository;
            _mapper = mapper;
        }

        public async Task<HospitalDto> GetByIdAsync(int hospitalId)
        {
            var hospital = await _hospitalRepository.GetByIdAsync(hospitalId);
            return _mapper.Map<HospitalDto>(hospital);
        }

        public async Task<IEnumerable<HospitalDto>> GetAllAsync()
        {
            var hospitals = await _hospitalRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<HospitalDto>>(hospitals);
        }

        public async Task AddAsync(HospitalDto hospitalDto)
        {
            var hospital = _mapper.Map<Hospital>(hospitalDto);
            await _hospitalRepository.AddAsync(hospital);
        }

        public async Task UpdateAsync(HospitalDto hospitalDto)
        {
            var hospital = _mapper.Map<Hospital>(hospitalDto);
            await _hospitalRepository.UpdateAsync(hospital);
        }

        public async Task DeleteAsync(int hospitalId)
        {
            await _hospitalRepository.DeleteAsync(hospitalId);
        }
    }
}

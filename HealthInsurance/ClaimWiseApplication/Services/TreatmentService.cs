using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Application.Interfaces;

namespace ClaimWise.Application.Services
{
    public class TreatmentService : ITreatmentService
    {
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IMapper _mapper;

        public TreatmentService(ITreatmentRepository treatmentRepository, IMapper mapper)
        {
            _treatmentRepository = treatmentRepository;
            _mapper = mapper;
        }

        public async Task<TreatmentDto> GetByIdAsync(int treatmentId)
        {
            var treatment = await _treatmentRepository.GetByIdAsync(treatmentId);
            return _mapper.Map<TreatmentDto>(treatment);
        }

        public async Task<IEnumerable<TreatmentDto>> GetAllAsync()
        {
            var treatments = await _treatmentRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TreatmentDto>>(treatments);
        }

        public async Task AddAsync(TreatmentDto treatmentDto)
        {
            var treatment = _mapper.Map<Treatment>(treatmentDto);
            await _treatmentRepository.AddAsync(treatment);
        }

        public async Task UpdateAsync(TreatmentDto treatmentDto)
        {
            var treatment = _mapper.Map<Treatment>(treatmentDto);
            await _treatmentRepository.UpdateAsync(treatment);
        }

        public async Task DeleteAsync(int treatmentId)
        {
            await _treatmentRepository.DeleteAsync(treatmentId);
        }
    }
}

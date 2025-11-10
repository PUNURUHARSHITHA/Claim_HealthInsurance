using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Domain.Entities;
using ClaimWise.Application.DTOs;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClaimWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class TreatmentController : ControllerBase
    {
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IMapper _mapper;

        public TreatmentController(ITreatmentRepository treatmentRepository, IMapper mapper)
        {
            _treatmentRepository = treatmentRepository;
            _mapper = mapper;
        }

        // ✅ GET: /api/Treatment
        [HttpGet]
        [Authorize(Roles = "Admin,Policyholder")] //Admin and PolicyHolder can access
        public async Task<IActionResult> GetAll()
        {
            var treatments = await _treatmentRepository.GetAllAsync();
            var treatmentDtos = _mapper.Map<IEnumerable<TreatmentDto>>(treatments);
            return Ok(treatmentDtos);
        }

        // ✅ GET: /api/Treatment/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] // ✅ Restrict entire controller to Admins
        public async Task<IActionResult> GetById(int id)
        {
            var treatment = await _treatmentRepository.GetByIdAsync(id);
            if (treatment == null)
                return NotFound();

            var treatmentDto = _mapper.Map<TreatmentDto>(treatment);
            return Ok(treatmentDto);
        }

        // ✅ POST: /api/Treatment
        [HttpPost]
        [Authorize(Roles = "Admin")] // ✅ Restrict entire controller to Admins
        public async Task<IActionResult> Create([FromBody] TreatmentDto treatmentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var treatment = new Treatment
            {
                TreatmentName = treatmentDto.TreatmentName,
                IsCovered = treatmentDto.IsCovered,
                WaitingPeriodMonths = treatmentDto.WaitingPeriodMonths
            };

            await _treatmentRepository.AddAsync(treatment);

            return CreatedAtAction(nameof(GetById), new { id = treatment.TreatmentID }, _mapper.Map<TreatmentDto>(treatment));
        }

        // ✅ PUT: /api/Treatment/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // ✅ Restrict entire controller to Admins
        public async Task<IActionResult> Update(int id, [FromBody] TreatmentDto treatmentDto)
        {
            if (id != treatmentDto.TreatmentID)
                return BadRequest("Treatment ID mismatch.");

            var treatment = _mapper.Map<Treatment>(treatmentDto);
            await _treatmentRepository.UpdateAsync(treatment);
            return NoContent();
        }

        // ✅ DELETE: /api/Treatment/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // ✅ Restrict entire controller to Admins
        public async Task<IActionResult> Delete(int id)
        {
            await _treatmentRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}

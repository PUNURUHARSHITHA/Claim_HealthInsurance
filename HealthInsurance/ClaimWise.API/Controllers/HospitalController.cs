
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HospitalController : ControllerBase
    {
        private readonly IHospitalRepository _hospitalRepository;
        private readonly IMapper _mapper;

        public HospitalController(IHospitalRepository hospitalRepository, IMapper mapper)
        {
            _hospitalRepository = hospitalRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Policyholder")]
        public async Task<IActionResult> GetAll()
        {
            var hospitals = await _hospitalRepository.GetAllAsync();
            var hospitalDtos = _mapper.Map<IEnumerable<HospitalDto>>(hospitals);
            return Ok(hospitalDtos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var hospital = await _hospitalRepository.GetByIdAsync(id);
            if (hospital == null)
                return NotFound();

            var hospitalDto = _mapper.Map<HospitalDto>(hospital);
            return Ok(hospitalDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] HospitalDto hospitalDto)
        {
            if (hospitalDto == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hospital = new Hospital
            {
                HospitalName = hospitalDto.HospitalName,
                Location = hospitalDto.Location,
                IsNetworkHospital = hospitalDto.IsNetworkHospital
            };

            await _hospitalRepository.AddAsync(hospital);

            return CreatedAtAction(nameof(GetById), new { id = hospital.HospitalID }, _mapper.Map<HospitalDto>(hospital));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] HospitalDto hospitalDto)
        {
            if (hospitalDto == null)
                return BadRequest();

            if (id != hospitalDto.HospitalID)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hospital = _mapper.Map<Hospital>(hospitalDto);
            await _hospitalRepository.UpdateAsync(hospital);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _hospitalRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}

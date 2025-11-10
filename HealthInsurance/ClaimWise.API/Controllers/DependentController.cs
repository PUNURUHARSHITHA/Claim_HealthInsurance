//using Microsoft.AspNetCore.Mvc;
//using ClaimWise.Domain.Entities;
//using ClaimWise.Domain.Interfaces;
//using ClaimWise.Application.DTOs;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace ClaimWise.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class DependentController : ControllerBase
//    {
//        private readonly IDependentRepository _dependentRepository;

//        public DependentController(IDependentRepository dependentRepository)
//        {
//            _dependentRepository = dependentRepository;
//        }

//        // GET: api/Dependent
//        [HttpGet]
//        [ProducesResponseType(typeof(IEnumerable<Dependent>), StatusCodes.Status200OK)]
//        public async Task<IActionResult> GetAll()
//        {
//            var dependents = await _dependentRepository.GetAllAsync();
//            return Ok(dependents);
//        }

//        // GET: api/Dependent/{dependentId}
//        [HttpGet("{dependentId}")]
//        [ProducesResponseType(typeof(Dependent), StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public async Task<IActionResult> GetById(int dependentId)
//        {
//            var dependent = await _dependentRepository.GetByIdAsync(dependentId);
//            if (dependent == null)
//                return NotFound($"Dependent with ID {dependentId} not found.");

//            return Ok(dependent);
//        }

//        // POST: api/Dependent
//        [HttpPost]
//        [ProducesResponseType(typeof(Dependent), StatusCodes.Status201Created)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        public async Task<IActionResult> Create([FromBody] DependentDto dependentDto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var dependent = new Dependent
//            {
//                Name = dependentDto.Name,
//                Relationship = dependentDto.Relationship,
//                PolicyholderID = dependentDto.PolicyholderID
//            };

//            await _dependentRepository.AddAsync(dependent);

//            return CreatedAtAction(nameof(GetById), new { dependentId = dependent.DependentID }, dependent);
//        }

//        // PUT: api/Dependent/{dependentId}
//        [HttpPut("{dependentId}")]
//        [ProducesResponseType(StatusCodes.Status204NoContent)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public async Task<IActionResult> Update(int dependentId, [FromBody] DependentDto dependentDto)
//        {
//            if (dependentId != dependentDto.DependentID)
//                return BadRequest("Dependent ID mismatch.");

//            var existing = await _dependentRepository.GetByIdAsync(dependentId);
//            if (existing == null)
//                return NotFound($"Dependent with ID {dependentId} not found.");

//            existing.Name = dependentDto.Name;
//            existing.Relationship = dependentDto.Relationship;
//            existing.PolicyholderID = dependentDto.PolicyholderID;

//            await _dependentRepository.UpdateAsync(existing);
//            return NoContent();
//        }

//        // DELETE: api/Dependent/{dependentId}
//        [HttpDelete("{dependentId}")]
//        [ProducesResponseType(StatusCodes.Status204NoContent)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        public async Task<IActionResult> Delete(int dependentId)
//        {
//            var existing = await _dependentRepository.GetByIdAsync(dependentId);
//            if (existing == null)
//                return NotFound($"Dependent with ID {dependentId} not found.");

//            await _dependentRepository.DeleteAsync(dependentId);
//            return NoContent();
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;

namespace ClaimWise.Application.Interfaces
{
    public interface ITreatmentService
    {
        Task<TreatmentDto> GetByIdAsync(int treatmentId);
        Task<IEnumerable<TreatmentDto>> GetAllAsync();
        Task AddAsync(TreatmentDto treatment);
        Task UpdateAsync(TreatmentDto treatment);
        Task DeleteAsync(int treatmentId);
    }
}

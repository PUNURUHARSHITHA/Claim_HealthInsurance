using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Domain.Interfaces
{
    public interface ITreatmentRepository
    {
        Task<Treatment> GetByIdAsync(int treatmentId);
        Task<IEnumerable<Treatment>> GetAllAsync();
        Task AddAsync(Treatment treatment);
        Task UpdateAsync(Treatment treatment);
        Task DeleteAsync(int treatmentId);
    }

}

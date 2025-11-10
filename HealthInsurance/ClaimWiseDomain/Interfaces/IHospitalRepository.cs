using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Domain.Interfaces
{
    public interface IHospitalRepository
    {
        Task<Hospital> GetByIdAsync(int hospitalId);
        Task<IEnumerable<Hospital>> GetAllAsync();
        Task AddAsync(Hospital hospital);
        Task UpdateAsync(Hospital hospital);
        Task DeleteAsync(int hospitalId);
    }

}

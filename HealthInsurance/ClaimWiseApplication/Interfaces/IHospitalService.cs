using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;

namespace ClaimWise.Application.Interfaces
{
    public interface IHospitalService
    {
        Task<HospitalDto> GetByIdAsync(int hospitalId);
        Task<IEnumerable<HospitalDto>> GetAllAsync();
        Task AddAsync(HospitalDto hospital);
        Task UpdateAsync(HospitalDto hospital);
        Task DeleteAsync(int hospitalId);
    }
}


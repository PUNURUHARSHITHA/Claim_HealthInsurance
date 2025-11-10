using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Domain.Interfaces
{
    public interface IPolicyholderRepository
    {
        Task<IEnumerable<Policyholder>> GetAllAsync();
        Task<Policyholder?> GetByIdAsync(int id);
        Task AddAsync(Policyholder policyholder);
        Task UpdateAsync(Policyholder policyholder);
        Task DeleteAsync(int id);
        //Task<Policyholder?> GetByPhoneNumberAsync(string phoneNumber); // ✅ New method

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDependentRepository
{
    Task<IEnumerable<Dependent>> GetAllAsync();
    Task<Dependent> GetByIdAsync(int id);
  
    Task AddAsync(Dependent dependent);
    Task UpdateAsync(Dependent dependent);
    Task DeleteAsync(int id);
}

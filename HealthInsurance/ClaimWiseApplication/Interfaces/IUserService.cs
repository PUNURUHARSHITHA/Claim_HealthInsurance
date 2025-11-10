using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Application.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(RegisterDto dto);
        Task<User?> ValidateCredentialsAsync(LoginDto dto);
    }

}

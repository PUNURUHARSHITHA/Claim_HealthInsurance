using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimWise.Application.DTOs;



namespace ClaimWise.Application.Interfaces
{
    public interface IReportService
    {
        Task<ClaimsReportDto> GetByIdAsync(int reportId);
        Task<IEnumerable<ClaimsReportDto>> GetAllAsync();
        Task AddAsync(ClaimsReportDto reportDto);
        Task<ClaimsReportDto> GenerateFraudReportAsync(int claimId);
       // Task<IEnumerable<ClaimsReportDto>> GenerateAllFraudReportsAsync();
    }
}

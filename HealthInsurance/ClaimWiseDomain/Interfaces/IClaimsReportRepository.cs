using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimWise.Domain.Entities;

namespace ClaimWise.Domain.Interfaces
{
    public interface IClaimsReportRepository
    {
        // ✅ Get all reports with related claim and policyholder
        Task<IEnumerable<ClaimsReport>> GetAllAsync();

        // ✅ Get a single report by ID
        Task<ClaimsReport?> GetByIdAsync(int id);

        // ✅ Add a new report (manual or fraud-generated)
        Task AddAsync(ClaimsReport report);

        // ✅ Update an existing report
        Task UpdateAsync(ClaimsReport report);

        // ✅ Delete a report by ID
        Task DeleteAsync(int id);

        // ✅ Filter reports by type (e.g., Fraud, Volume)
        Task<IEnumerable<ClaimsReport>> GetByTypeAsync(string type);

        // ✅ Filter reports by date range (for audit and compliance)
        Task<IEnumerable<ClaimsReport>> GetByDateRangeAsync(DateTime from, DateTime to);
    }
}

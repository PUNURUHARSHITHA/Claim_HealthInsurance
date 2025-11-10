using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ClaimWise.Application.DTOs;
using ClaimWise.Application.Interfaces;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   // [Authorize(Roles = "Admin")] // ✅ Restrict entire controller to Admins
    public class ClaimsReportController : ControllerBase
    {
        private readonly IClaimsReportRepository _claimsReportRepository;
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;

        public ClaimsReportController(
            IClaimsReportRepository claimsReportRepository,
            IReportService reportService,
            IMapper mapper)
        {
            _claimsReportRepository = claimsReportRepository;
            _reportService = reportService;
            _mapper = mapper;
        }

        // ✅ GET: All reports
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _claimsReportRepository.GetAllAsync();
            var reportDtos = _mapper.Map<IEnumerable<ClaimsReportDto>>(reports);
            return Ok(reportDtos);
        }

        // ✅ POST: Auto-generate  report for a specific claim
        [HttpPost("Auto-generateReport")]
        public async Task<IActionResult> GenerateFraudReport([FromBody] GenerateReportRequestDto dto)
        {
            if (dto == null || dto.ClaimId <= 0)
                return BadRequest("Invalid claim ID");

            var report = await _reportService.GenerateFraudReportAsync(dto.ClaimId);
            return Ok(report);
        }

        // ✅ GET: Reports by type
        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetByType(string type)
        {
            var reports = await _claimsReportRepository.GetByTypeAsync(type);
            var dtos = _mapper.Map<IEnumerable<ClaimsReportDto>>(reports);
            return Ok(dtos);
        }

        // ✅ GET: Reports by date range
        [HttpGet("range")]
        public async Task<IActionResult> GetByDateRange(DateTime from, DateTime to)
        {
            var reports = await _claimsReportRepository.GetByDateRangeAsync(from, to);
            var dtos = _mapper.Map<IEnumerable<ClaimsReportDto>>(reports);
            return Ok(dtos);
        }
        [HttpGet("download/{reportId}")]
        public async Task<IActionResult> DownloadReport(int reportId)
        {
            var reportDto = await _reportService.GetByIdAsync(reportId);
            if (reportDto == null)
                return NotFound($"Report with ID {reportId} not found.");

            var pdfBytes = GeneratePdf(reportDto);
            return File(pdfBytes, "application/pdf", $"ClaimReport_{reportId}.pdf");
        }

        // ✅ Helper: Generate simple PDF content (placeholder)
        private byte[] GeneratePdf(ClaimsReportDto report)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true);

            writer.WriteLine("ClaimWise Report");
            writer.WriteLine("-------------------------");
            writer.WriteLine($"Report ID: {report.ReportID}");
            writer.WriteLine($"Claim ID: {report.ClaimID}");
            writer.WriteLine($"Policyholder ID: {report.PolicyholderID}");
            writer.WriteLine($"Type: {report.Type}");
            writer.WriteLine($"Generated Date: {report.GeneratedDate}");
            writer.WriteLine($"Insights: {report.Insights}");
            writer.Flush();

            return ms.ToArray(); // Replace with real PDF generation later
        }
    }
}
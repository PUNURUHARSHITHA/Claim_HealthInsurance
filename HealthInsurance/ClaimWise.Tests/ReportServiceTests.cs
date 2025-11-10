using Xunit;
using Moq;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using ClaimWise.Application.Services;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;

public class ReportServiceTests
{
    private readonly Mock<IClaimsReportRepository> _reportRepo = new();
    private readonly Mock<IClaimRepository> _claimRepo = new();
    private readonly Mock<IEligibilityCheckRepository> _eligibilityRepo = new();
    private readonly IMapper _mapper;

    private readonly ReportService _service;

    public ReportServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ClaimsReport, ClaimsReportDto>().ReverseMap();
        });

        _mapper = config.CreateMapper();

        _service = new ReportService(
            _reportRepo.Object,
            _claimRepo.Object,
            _eligibilityRepo.Object,
            _mapper
        );
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMappedDto()
    {
        var report = new ClaimsReport { ClaimID = 1, Type = "Fraud", Insights = "Flagged" };
        _reportRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(report);

        var dto = await _service.GetByIdAsync(1);

        Assert.Equal("Fraud", dto.Type);
        Assert.Equal("Flagged", dto.Insights);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        var reports = new List<ClaimsReport>
        {
            new ClaimsReport { ClaimID = 1, Type = "Fraud" },
            new ClaimsReport { ClaimID = 2, Type = "Eligible" }
        };

        _reportRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Type == "Fraud");
        Assert.Contains(result, r => r.Type == "Eligible");
    }

    [Fact]
    public async Task AddAsync_SavesReportWithDefaults()
    {
        var dto = new ClaimsReportDto { ClaimID = 1 };

        await _service.AddAsync(dto);

        _reportRepo.Verify(r => r.AddAsync(It.Is<ClaimsReport>(cr =>
            cr.Type == "Manual" &&
            cr.Insights.Contains("Manual report created") &&
            cr.GeneratedDate.HasValue
        )), Times.Once);
    }

    [Fact]
    public async Task GenerateFraudReportAsync_ReturnsEligibilityFailure()
    {
        var claim = new Claim { ClaimID = 1, PolicyholderID = 10 };
        var check = new EligibilityCheck { Result = "Not Eligible", RuleApplied = "Rule X" };

        _claimRepo.Setup(r => r.GetByIdWithPolicyAsync(1)).ReturnsAsync(claim);
        _eligibilityRepo.Setup(r => r.GetLatestByClaimIdAsync(1)).ReturnsAsync(check);

        var result = await _service.GenerateFraudReportAsync(1);

        Assert.Equal("EligibilityFailure", result.Type);
        Assert.Contains("failed eligibility check", result.Insights);
        _reportRepo.Verify(r => r.AddAsync(It.IsAny<ClaimsReport>()), Times.Once);
    }

    [Fact]
    public async Task GenerateFraudReportAsync_ReturnsFraud_WhenFraudReasonExists()
    {
        var claim = new Claim { ClaimID = 2, PolicyholderID = 20, FraudReason = "Suspicious billing" };

        _claimRepo.Setup(r => r.GetByIdWithPolicyAsync(2)).ReturnsAsync(claim);
        _eligibilityRepo.Setup(r => r.GetLatestByClaimIdAsync(2)).ReturnsAsync((EligibilityCheck)null);

        var result = await _service.GenerateFraudReportAsync(2);

        Assert.Equal("Fraud", result.Type);
        Assert.Contains("Suspicious billing", result.Insights);
    }

    
    [Fact]
    public async Task GenerateFraudReportAsync_ThrowsException_WhenClaimNotFound()
    {
        _claimRepo.Setup(r => r.GetByIdWithPolicyAsync(99)).ReturnsAsync((Claim)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateFraudReportAsync(99));
    }
}

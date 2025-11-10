using Xunit;
using Moq;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClaimWise.Application.Services;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;

public class EligibilityCheckServiceTests
{
    private readonly Mock<IEligibilityCheckRepository> _eligibilityRepoMock = new();
    private readonly Mock<IClaimRepository> _claimRepoMock = new();
    private readonly IMapper _mapper;

    public EligibilityCheckServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<EligibilityCheck, EligibilityCheckDto>().ReverseMap();
            cfg.CreateMap<Claim, ClaimDto>();
        });
        _mapper = config.CreateMapper();
    }

    private EligibilityCheckService CreateService() =>
        new EligibilityCheckService(_eligibilityRepoMock.Object, _claimRepoMock.Object, _mapper);

    [Fact]
    public async Task AllRulesPass_ReturnsEligible()
    {
        var claim = new Claim
        {
            ClaimID = 1,
            Treatment = new Treatment { IsCovered = true, WaitingPeriodMonths = 0 },
            Hospital = new Hospital { IsNetworkHospital = true },
            Policyholder = new Policyholder { PolicyStartDate = DateTime.UtcNow.AddMonths(-6) },
            SubmissionDate = DateTime.UtcNow,
            Status = "UnderReview",
            Documents = "doc.pdf"
        };

        _claimRepoMock.Setup(r => r.GetByIdWithPolicyholderAsync(1)).ReturnsAsync(claim);
        _claimRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Claim>());

        var service = CreateService();
        var result = await service.RunEligibilityCheckAsync(1, "TestUser");

        Assert.Equal("Eligible", result.Result);
        Assert.Contains("All rules passed", result.RulesApplied);
    }

    [Fact]
    public async Task StatusNotUnderReview_ReturnsNotEligible()
    {
        var claim = new Claim
        {
            ClaimID = 2,
            Treatment = new Treatment { IsCovered = true, WaitingPeriodMonths = 0 },
            Hospital = new Hospital { IsNetworkHospital = true },
            Policyholder = new Policyholder { PolicyStartDate = DateTime.UtcNow.AddMonths(-6) },
            SubmissionDate = DateTime.UtcNow,
            Status = "Pending",
            Documents = "doc.pdf"
        };

        _claimRepoMock.Setup(r => r.GetByIdWithPolicyholderAsync(2)).ReturnsAsync(claim);
        _claimRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Claim>());

        var service = CreateService();
        var result = await service.RunEligibilityCheckAsync(2, "TestUser");

        Assert.Equal("Not Eligible", result.Result);
        Assert.True(result.RulesApplied.Any(r => r.Contains("Claim must be under review")));
    }
}

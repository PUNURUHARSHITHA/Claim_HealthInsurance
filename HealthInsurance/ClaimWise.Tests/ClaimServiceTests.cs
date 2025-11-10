using Xunit;
using Moq;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimWise.Application.Services;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;

public class ClaimServiceTests
{
    private readonly Mock<IClaimRepository> _claimRepoMock = new();
    private readonly Mock<IPolicyholderRepository> _policyholderRepoMock = new();
    private readonly Mock<IWhatsAppService> _whatsAppServiceMock = new();
    private readonly Mock<IClaimActionLogService> _logServiceMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly IMapper _mapper;

    public ClaimServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Claim, ClaimDto>().ReverseMap();
        });
        _mapper = config.CreateMapper();
    }

    private ClaimService CreateService() =>
        new ClaimService(_claimRepoMock.Object, _policyholderRepoMock.Object, _mapper,
                         _whatsAppServiceMock.Object, _configMock.Object, _logServiceMock.Object);

    [Fact]
    public async Task GetClaimByIdAsync_ValidId_ReturnsClaimDto()
    {
        var claim = new Claim { ClaimID = 1, Status = "Submitted" };
        _claimRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(claim);

        var service = CreateService();
        var result = await service.GetClaimByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.ClaimID);
    }

    [Fact]
    public async Task GetClaimByIdAsync_InvalidId_ReturnsNull()
    {
        _claimRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Claim)null);

        var service = CreateService();
        var result = await service.GetClaimByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateClaimStatusAsync_ValidStatus_UpdatesClaim()
    {
        var claim = new Claim { ClaimID = 2, Status = "Submitted" };
        _claimRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(claim);

        var dto = new UpdateClaimStatusDto { Status = "UnderReview", UpdatedBy = "Admin" };
        var service = CreateService();
        var result = await service.UpdateClaimStatusAsync(2, dto);

        Assert.True(result);
        Assert.Equal("UnderReview", claim.Status);
        _logServiceMock.Verify(l => l.LogAsync(2, "Admin", "Status changed to UnderReview"), Times.Once);
    }

    [Fact]
    public async Task UpdateClaimStatusAsync_InvalidStatus_ReturnsFalse()
    {
        var claim = new Claim { ClaimID = 3, Status = "Submitted" };
        _claimRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(claim);

        var dto = new UpdateClaimStatusDto { Status = "InvalidStatus", UpdatedBy = "Admin" };
        var service = CreateService();
        var result = await service.UpdateClaimStatusAsync(3, dto);

        Assert.False(result);
    }

    [Fact]
    public async Task ApproveClaimAsync_ValidClaim_ApprovesSuccessfully()
    {
        var claim = new Claim { ClaimID = 4, Status = "UnderReview" };
        _claimRepoMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(claim);

        var service = CreateService();
        var result = await service.ApproveClaimAsync(4, "Manager");

        Assert.True(result);
        Assert.Equal("Approved", claim.Status);
        _logServiceMock.Verify(l => l.LogAsync(4, "Manager", "Claim Approved"), Times.Once);
    }

    [Fact]
    public async Task ApproveClaimAsync_AlreadyApproved_ReturnsFalse()
    {
        var claim = new Claim { ClaimID = 5, Status = "Approved" };
        _claimRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(claim);

        var service = CreateService();
        var result = await service.ApproveClaimAsync(5, "Manager");

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteClaimAsync_ValidId_DeletesClaim()
    {
        var claim = new Claim { ClaimID = 6 };
        _claimRepoMock.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(claim);

        var service = CreateService();
        var result = await service.DeleteClaimAsync(6);

        Assert.True(result);
        _claimRepoMock.Verify(r => r.DeleteAsync(6), Times.Once);
    }

    [Fact]
    public async Task DeleteClaimAsync_InvalidId_ReturnsFalse()
    {
        _claimRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Claim)null);

        var service = CreateService();
        var result = await service.DeleteClaimAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task NotifyPolicyholderStatusChangeAsync_ValidPhone_SendsMessage()
    {
        var claim = new Claim { ClaimID = 7, Status = "Approved", PolicyholderID = 100 };
        var policyholder = new Policyholder { Name = "Rohith", PhoneNumber = "9347916900" };

        _policyholderRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(policyholder);
        _whatsAppServiceMock.Setup(w => w.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.NotifyPolicyholderStatusChangeAsync(claim);

        _whatsAppServiceMock.Verify(w => w.SendMessageAsync(It.Is<string>(n => n.Contains("91")),
            It.Is<string>(m => m.Contains("approved"))), Times.Once);
    }

    [Fact]
    public async Task GetClaimByPolicyholderIdAsync_ValidId_ReturnsClaimDto()
    {
        var claim = new Claim { ClaimID = 8, PolicyholderID = 200 };
        _claimRepoMock.Setup(r => r.GetByPolicyholderIdAsync(200)).ReturnsAsync(claim);

        var service = CreateService();
        var result = await service.GetClaimByPolicyholderIdAsync(200);

        Assert.NotNull(result);
        Assert.Equal(8, result.ClaimID);
    }

    [Fact]
    public async Task GetClaimStatusCountsAsync_ReturnsDictionary()
    {
        var statusCounts = new Dictionary<string, int>
        {
            { "Submitted", 5 },
            { "Approved", 3 }
        };

        _claimRepoMock.Setup(r => r.GetClaimStatusCountsAsync()).ReturnsAsync(statusCounts);

        var service = CreateService();
        var result = await service.GetClaimStatusCountsAsync();

        Assert.Equal(5, result["Submitted"]);
        Assert.Equal(3, result["Approved"]);
    }
}

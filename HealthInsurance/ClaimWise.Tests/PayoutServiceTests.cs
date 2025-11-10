using Xunit;
using Moq;
using AutoMapper;
using System;
using System.Threading.Tasks;
using ClaimWise.Application.Services;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Application.Interfaces;

public class PayoutServiceTests
{
    private readonly Mock<IPayoutRepository> _payoutRepoMock = new();
    private readonly Mock<IClaimRepository> _claimRepoMock = new();
    private readonly Mock<IClaimActionLogService> _logServiceMock = new();
    private readonly Mock<IWhatsAppService> _whatsAppServiceMock = new();
    private readonly Mock<IPolicyholderRepository> _policyholderRepoMock = new();
    private readonly IMapper _mapper;

    public PayoutServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Payout, PayoutDto>().ReverseMap();
        });
        _mapper = config.CreateMapper();
    }

    private PayoutService CreateService() =>
        new PayoutService(_payoutRepoMock.Object, _claimRepoMock.Object, _logServiceMock.Object,
                          _whatsAppServiceMock.Object, _policyholderRepoMock.Object, _mapper);

    [Fact]
    public async Task CalculatePayoutAsync_ValidClaim_ReturnsCorrectAmount()
    {
        var claim = new Claim
        {
            ClaimID = 30001,
            Policyholder = new Policyholder
            {
                PolicyType = new PolicyType { CoverageLimit = 10000 }
            }
        };

        _claimRepoMock.Setup(r => r.GetByIdWithPolicyAsync(30001)).ReturnsAsync(claim);

        var service = CreateService();
        var result = await service.CalculatePayoutAsync(30001);

        Assert.Equal(8000, result.Amount);
        Assert.Equal("Pending", result.ApprovalStatus);
        _payoutRepoMock.Verify(r => r.AddAsync(It.IsAny<Payout>()), Times.Once);
        _logServiceMock.Verify(l => l.LogAsync(30001, "System", It.Is<string>(s => s.Contains("₹8000"))), Times.Once);
    }

    [Fact]
    public async Task CalculatePayoutAsync_InvalidClaim_ReturnsZero()
    {
        _claimRepoMock.Setup(r => r.GetByIdWithPolicyAsync(999)).ReturnsAsync((Claim)null);

        var service = CreateService();
        var result = await service.CalculatePayoutAsync(999);

        Assert.Equal(0, result.Amount);
        Assert.Equal("Claim not found or missing policy details", result.ApprovalStatus);
    }

    [Fact]
    public async Task GetPayoutDetailsAsync_CompletedPayout_ReturnsCorrectStatus()
    {
        var payout = new Payout
        {
            ClaimID = 30001,
            Amount = 5000,
            ApprovalStatus = "Completed",
            PayoutStatus = "Transferred",
            TransferDate = DateTime.Parse("2025-10-17 01:00:00"),
            BankReferenceNumber = "A3DCE122345"
        };

        _payoutRepoMock.Setup(r => r.GetByClaimIdAsync(30001)).ReturnsAsync(payout);

        var service = CreateService();
        var result = await service.GetPayoutDetailsAsync(30001);

        Assert.Equal("Completed", result.ApprovalStatus);
        Assert.Equal("Transferred", payout.PayoutStatus);
        Assert.Equal("A3DCE122345", payout.BankReferenceNumber);
    }

    [Fact]
    public async Task ApproveLevel2Async_Level1ApprovedOnly_UpdatesLevel2()
    {
        var payout = new Payout
        {
            PayoutID = 2,
            ClaimID = 30002,
            ApprovalStatus = "Level1 Approved",
            ApprovedByLevel1 = "Pratheek"
        };

        _payoutRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(payout);

        var service = CreateService();
        await service.ApproveLevel2Async(2, "Lance");

        Assert.Equal("Level 2 Approved", payout.ApprovalStatus);
        Assert.Equal("Lance", payout.ApprovedByLevel2);
        _logServiceMock.Verify(l => l.LogAsync(30002, "Lance", "Payout Level 2 Approved"), Times.Once);
    }

    [Fact]
    public async Task MarkAsTransferredAsync_ValidBankRef_UpdatesTransferFields()
    {
        var payout = new Payout { PayoutID = 3, ClaimID = 40001 };
        _payoutRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(payout);

        var service = CreateService();
        await service.MarkAsTransferredAsync(3, "B2C0A122345", "FinanceTeam");

        Assert.Equal("Transferred", payout.PayoutStatus);
        Assert.Equal("Success", payout.TransferStatus);
        Assert.Equal("Completed", payout.ApprovalStatus);
        Assert.Equal("B2C0A122345", payout.BankReferenceNumber);
        _logServiceMock.Verify(l => l.LogAsync(40001, "FinanceTeam", It.Is<string>(s => s.Contains("B2C0A122345"))), Times.Once);
    }

    [Fact]
    public async Task FullApprovalAndTransferFlow_ValidPayout_TransitionsCorrectly()
    {
        var payout = new Payout { PayoutID = 4, ClaimID = 40002 };
        _payoutRepoMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(payout);

        var service = CreateService();
        await service.ApproveLevel1Async(4, "Pratheek");
        await service.ApproveLevel2Async(4, "Lance");
        await service.MarkAsTransferredAsync(4, "E5A0C412256", "FinanceTeam");

        Assert.Equal("Completed", payout.ApprovalStatus);
        Assert.Equal("Transferred", payout.PayoutStatus);
        Assert.Equal("Success", payout.TransferStatus);
        Assert.Equal("E5A0C412256", payout.BankReferenceNumber);

        _logServiceMock.Verify(l => l.LogAsync(40002, "Pratheek", "Payout Level 1 Approved"), Times.Once);
        _logServiceMock.Verify(l => l.LogAsync(40002, "Lance", "Payout Level 2 Approved"), Times.Once);
        _logServiceMock.Verify(l => l.LogAsync(40002, "FinanceTeam", It.Is<string>(s => s.Contains("Transferred"))), Times.Once);
    }

    [Fact]
    public async Task NotifyPolicyholderPayoutAsync_ValidPhone_SendsMessage()
    {
        var payout = new Payout { PayoutID = 5, ClaimID = 40003 };
        var claim = new Claim
        {
            ClaimID = 40003,
            Policyholder = new Policyholder { Name = "Rohith", PhoneNumber = "9347916900" }
        };

        _claimRepoMock.Setup(r => r.GetByIdWithPolicyAsync(40003)).ReturnsAsync(claim);
        _whatsAppServiceMock.Setup(w => w.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.NotifyPolicyholderPayoutAsync(payout);

        _whatsAppServiceMock.Verify(w => w.SendMessageAsync(It.Is<string>(n => n.Contains("91")), It.Is<string>(m => m.Contains("successfully transferred"))), Times.Once);
    }

    [Fact]
    public async Task NotifyPolicyholderLevel1ApprovalAsync_ValidPhone_SendsMessage()
    {
        var payout = new Payout { PayoutID = 6, ClaimID = 40004 };
        var claim = new Claim
        {
            ClaimID = 40004,
            Policyholder = new Policyholder { Name = "Rohith", PhoneNumber = "9347916900" }
        };

        _claimRepoMock.Setup(r => r.GetByIdWithPolicyAsync(40004)).ReturnsAsync(claim);
        _whatsAppServiceMock.Setup(w => w.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.NotifyPolicyholderLevel1ApprovalAsync(payout);

        _whatsAppServiceMock.Verify(w => w.SendMessageAsync(It.Is<string>(n => n.Contains("91")), It.Is<string>(m => m.Contains("approved at Level 1"))), Times.Once);
    }
}

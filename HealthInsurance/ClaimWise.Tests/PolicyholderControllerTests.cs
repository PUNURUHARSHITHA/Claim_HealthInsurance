using Xunit;
using Moq;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ClaimWise.API.Controllers;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;

using SecurityClaim = System.Security.Claims.Claim;

public class PolicyholderControllerTests
{
    private readonly Mock<IPolicyholderRepository> _policyholderRepoMock = new();
    private readonly Mock<IDependentRepository> _dependentRepoMock = new();
    private readonly Mock<IAgentRepository> _agentRepoMock = new();
    private readonly Mock<IPolicyTypeRepository> _policyTypeRepoMock = new();
    private readonly IMapper _mapper;

    public object SecurityClaimTypes { get; private set; }

    public PolicyholderControllerTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Policyholder, PolicyholderDto>().ReverseMap();
            cfg.CreateMap<Dependent, DependentDto>().ReverseMap();
        });
        _mapper = config.CreateMapper();
    }

    private PolicyholderController CreateController(string role = "Policyholder", string username = "Rohith")
    {
        var controller = new PolicyholderController(
            _policyholderRepoMock.Object,
            _dependentRepoMock.Object,
            _agentRepoMock.Object,
            _policyTypeRepoMock.Object,
            _mapper);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new SecurityClaim(ClaimTypes.Name, username),
            new SecurityClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role)
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Fact]
    public async Task GetAll_ReturnsPolicyholders()
    {
        var policyholders = new List<Policyholder>
        {
            new Policyholder { PolicyholderID = 1, Name = "Rohith" },
            new Policyholder { PolicyholderID = 2, Name = "Sumith" }
        };

        _policyholderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(policyholders);

        var controller = CreateController("Admin");
        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<PolicyholderDto>>(ok.Value);
        Assert.Equal(2, ((List<PolicyholderDto>)returned).Count);
    }

    [Fact]
    public async Task Create_ValidPolicyholder_ReturnsCreated()
    {
        var dto = new PolicyholderDto
        {
            PolicyTypeID = 1,
            AgentID = 10,
            Region = "South",
            ProductType = "Health",
            PhoneNumber = "9347916900",
            BankReferenceNumber = "BANK123",
            PolicyStartDate = System.DateTime.UtcNow
        };

        var agents = new List<Agent> { new Agent { AgentID = 10 } };
        var policyType = new PolicyType { PolicyTypeID = 1, PolicyDurationYears = 5, Description = "Standard Coverage" };

        _agentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(agents);
        _policyTypeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policyType);

        var controller = CreateController("Policyholder", "Rohith");
        var result = await controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var returned = Assert.IsType<PolicyholderDto>(created.Value);
        Assert.Equal("Rohith", returned.Name);
        Assert.Equal("Standard Coverage", returned.CoverageDetails);
    }

    [Fact]
    public async Task Create_InvalidAgent_ReturnsBadRequest()
    {
        var dto = new PolicyholderDto
        {
            PolicyTypeID = 1,
            AgentID = 99,
            PolicyStartDate = System.DateTime.UtcNow
        };

        _agentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Agent> { new Agent { AgentID = 10 } });

        var controller = CreateController("Policyholder", "Rohith");
        var result = await controller.Create(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Invalid Agent ID", badRequest.Value.ToString());
    }

    [Fact]
    public async Task Create_InvalidPolicyType_ReturnsBadRequest()
    {
        var dto = new PolicyholderDto
        {
            PolicyTypeID = 99,
            AgentID = 10,
            PolicyStartDate = System.DateTime.UtcNow
        };

        _agentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Agent> { new Agent { AgentID = 10 } });
        _policyTypeRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((PolicyType)null);

        var controller = CreateController("Policyholder", "Rohith");
        var result = await controller.Create(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Invalid PolicyTypeID", badRequest.Value.ToString());
    }

    [Fact]
    public async Task AddDependent_ValidRequest_ReturnsOk()
    {
        var dto = new DependentDto { Name = "Child", Relationship = "Son" };
        var policyholder = new Policyholder { PolicyholderID = 1, Name = "Rohith" };

        _policyholderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(policyholder);

        var controller = CreateController("Policyholder", "Rohith");
        var result = await controller.AddDependent(1, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Dependent added successfully", ok.Value);
    }

    [Fact]
    public async Task AddDependent_InvalidPolicyholder_ReturnsNotFound()
    {
        _policyholderRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Policyholder)null);

        var dto = new DependentDto { Name = "Child", Relationship = "Son" };
        var controller = CreateController("Policyholder", "Rohith");
        var result = await controller.AddDependent(99, dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFound.Value.ToString());
    }
}

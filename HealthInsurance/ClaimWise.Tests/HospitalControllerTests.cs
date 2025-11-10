using Xunit;
using Moq;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ClaimWise.API.Controllers;
using ClaimWise.Application.DTOs;
using ClaimWise.Domain.Entities;
using ClaimWise.Domain.Interfaces;

public class HospitalControllerTests
{
    private readonly Mock<IHospitalRepository> _hospitalRepoMock = new();
    private readonly IMapper _mapper;

    public HospitalControllerTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Hospital, HospitalDto>().ReverseMap();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        var controller = new HospitalController(_hospitalRepoMock.Object, _mapper);

        var result = await controller.Create(null);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_NullDto_ReturnsBadRequest()
    {
        var controller = new HospitalController(_hospitalRepoMock.Object, _mapper);

        var result = await controller.Update(1, null);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_InvalidModelState_ReturnsBadRequest()
    {
        var controller = new HospitalController(_hospitalRepoMock.Object, _mapper);
        controller.ModelState.AddModelError("Location", "Required");

        var dto = new HospitalDto
        {
            HospitalID = 1,
            HospitalName = "Apollo",
            Location = "",
            IsNetworkHospital = true
        };

        var result = await controller.Update(1, dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}

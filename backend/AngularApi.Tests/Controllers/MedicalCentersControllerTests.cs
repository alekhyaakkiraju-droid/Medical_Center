using AngularApi.Controllers;
using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AngularApi.Tests.Controllers;

public class MedicalCentersControllerTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly MedicalCentersController _controller;

    public MedicalCentersControllerTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _context = new MedicalCenterDbContext(options);
        _controller = new MedicalCentersController(
            new MedicalCenterService(_context, new OwnershipValidator(), NullLogger<MedicalCenterService>.Instance));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    }

    [Fact]
    public async Task GetMedicalCenter_ReturnsPagedMedicalCenters()
    {
        _context.MedicalCenter.AddRange(new MedicalCenter { Id = 1, City = "Boston" }, new MedicalCenter { Id = 2, City = "Cambridge" });
        await _context.SaveChangesAsync();
        var result = await _controller.GetMedicalCenter(new PaginationParameters());
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMedicalCenter_NonExistingId_ReturnsNotFound()
    {
        (await _controller.GetMedicalCenter(1)).Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task PutMedicalCenter_NonExistingId_ReturnsNotFound(){var dto=new UpdateMedicalCenterDTO{StreetAddress="1 Main St",City="Boston",State="MA",Zip="02101"};(await _controller.PutMedicalCenter(1,dto)).Should().BeOfType<NotFoundResult>();}

    [Fact]
    public async Task PostMedicalCenter_ValidInput_CreatesMedicalCenter(){var dto=new CreateMedicalCenterDTO{StreetAddress="1 Main St",City="Boston",State="MA",Zip="02101"};var result=await _controller.PostMedicalCenter(dto);result.Result.Should().BeOfType<CreatedAtActionResult>();(await _context.MedicalCenter.CountAsync()).Should().Be(1);}

    [Fact]
    public async Task DeleteMedicalCenter_ExistingId_DeletesMedicalCenter()
    {
        _context.MedicalCenter.Add(new MedicalCenter { Id = 1, City = "Boston" });
        await _context.SaveChangesAsync();
        (await _controller.DeleteMedicalCenter(1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteMedicalCenter_NonExistingId_ReturnsNotFound()
    {
        (await _controller.DeleteMedicalCenter(1)).Should().BeOfType<NotFoundResult>();
    }

    public void Dispose() { _context.Database.EnsureDeleted(); _context.Dispose(); }
}

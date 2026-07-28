using AngularApi.Controllers;
using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Options;
using AngularApi.Services.impelementation;
using AngularApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AngularApi.Tests.Controllers;

public class AppointmentsPaginationControllerTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly AppointmentsController _controller;

    public AppointmentsPaginationControllerTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MedicalCenterDbContext(options);
        var appointmentService = new AppointmentService(
            _context,
            Microsoft.Extensions.Options.Options.Create(new AppointmentSettings()));
        _controller = new AppointmentsController(
            appointmentService,
            null!,
            null!,
            null!);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "admin1"),
            new Claim(ClaimTypes.Role, "admin"),
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
        };

        for (var i = 1; i <= 15; i++)
        {
            _context.Appointments.Add(new Appointment
            {
                Id = i,
                DoctorName = "Dr Smith",
                PatientId = $"patient{i}",
                AppointmentTakenDate = DateTime.UtcNow.AddDays(i)
            });
        }

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAppointments_WithPageAndPageSize_ReturnsTenResultsAndMetadata()
    {
        var pagination = new PaginationParameters { Page = 1, PageSize = 10 };

        var result = await _controller.GetAppointments(pagination);

        var paged = result.Value.Should().NotBeNull().And.BeOfType<PagedResult<AppointmentDTO>>().Subject;
        paged.Items.Should().HaveCount(10);
        paged.TotalCount.Should().Be(15);
        paged.CurrentPage.Should().Be(1);
        paged.PageSize.Should().Be(10);
        paged.PageCount.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

using AngularApi.Models;
using AngularApi.Contracts.DTO;
using AngularApi.DTO;
using AngularApi.Contracts.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Tests.DTO;

public class QueryablePaginationExtensionsTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;

    public QueryablePaginationExtensionsTests()
    {
        var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MedicalCenterDbContext(options);

        for (var i = 1; i <= 25; i++)
        {
            _context.Appointments.Add(new Appointment
            {
                Id = i,
                DoctorName = $"Doctor {i}",
                PatientId = $"patient{i}"
            });
        }

        _context.SaveChanges();
    }

    [Fact]
    public async Task ToPagedResultAsync_ReturnsRequestedPageSizeAndMetadata()
    {
        var pagination = new PaginationParameters { Page = 1, PageSize = 10 };

        var result = await _context.Appointments
            .SelectAppointmentDto()
            .ToPagedResultAsync(pagination);

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.PageCount.Should().Be(3);
    }

    [Fact]
    public async Task ToPagedResultAsync_DefaultPaginationReturnsTwentyItems()
    {
        var result = await _context.Appointments
            .SelectAppointmentDto()
            .ToPagedResultAsync(new PaginationParameters());

        result.Items.Should().HaveCount(20);
        result.TotalCount.Should().Be(25);
        result.PageSize.Should().Be(20);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

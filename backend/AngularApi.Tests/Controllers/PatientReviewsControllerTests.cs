using AngularApi.Controllers;
using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace AngularApi.Tests.Controllers;

public class PatientReviewsControllerTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly PatientReviewsController _controller;

    public PatientReviewsControllerTests()
    {
        _context = new MedicalCenterDbContext(new DbContextOptionsBuilder<MedicalCenterDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        _controller = new PatientReviewsController(
            new PatientReviewService(_context, new OwnershipValidator(), NullLogger<PatientReviewService>.Instance));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "patient1"), new Claim(ClaimTypes.Role, "admin") }, "TestAuth")) } };
    }

    [Fact] public async Task GetPatientReviews_ReturnsAllReviews() { _context.PatientReviews.AddRange(new PatientReview { Id = 1, PatientId = "patient1", OverallRating = 5, Patient = new Patient { Id = "patient1", Name = "John" } }, new PatientReview { Id = 2, PatientId = "patient2", OverallRating = 4, Patient = new Patient { Id = "patient2", Name = "Jane" } }); await _context.SaveChangesAsync(); (await _controller.GetPatientReviews(new PaginationParameters())).Value!.Items.Should().HaveCount(2); }
    [Fact] public async Task GetPatientReview_ExistingId_ReturnsReview() { _context.PatientReviews.Add(new PatientReview { Id = 1, PatientId = "patient1", OverallRating = 5 }); await _context.SaveChangesAsync(); (await _controller.GetPatientReview(1)).Value!.OverallRating.Should().Be(5); }
    [Fact] public async Task GetPatientReview_NonExistingId_ReturnsNotFound() => (await _controller.GetPatientReview(1)).Result.Should().BeOfType<NotFoundResult>();
    [Fact] public async Task PutPatientReview_NonExistingId_ReturnsNotFound() => (await _controller.PutPatientReview(1, new UpdatePatientReviewDTO { OverallRating = 5 })).Should().BeOfType<NotFoundResult>();
    [Fact] public async Task PutPatientReview_ExistingId_UpdatesReview() { _context.PatientReviews.Add(new PatientReview { Id = 1, PatientId = "patient1", DoctorId = "doctor-1", OverallRating = 3 }); await _context.SaveChangesAsync(); (await _controller.PutPatientReview(1, new UpdatePatientReviewDTO { DoctorId = "doctor-1", OverallRating = 5 })).Should().BeOfType<NoContentResult>(); }
    [Fact] public async Task PostPatientReview_ValidInput_CreatesReview() { var created = (((CreatedAtActionResult)(await _controller.PostPatientReview(new CreatePatientReviewDTO { DoctorId = "doctor-1", OverallRating = 5 })).Result!).Value as PatientReview); created!.PatientId.Should().Be("patient1"); created.DoctorId.Should().Be("doctor-1"); }
    [Fact] public async Task DeletePatientReview_ExistingId_DeletesReview() { _context.PatientReviews.Add(new PatientReview { Id = 1, PatientId = "patient1" }); await _context.SaveChangesAsync(); (await _controller.DeletePatientReview(1)).Should().BeOfType<NoContentResult>(); (await _context.PatientReviews.FindAsync(1)).Should().BeNull(); }
    [Fact] public async Task DeletePatientReview_NonExistingId_ReturnsNotFound() => (await _controller.DeletePatientReview(1)).Should().BeOfType<NotFoundResult>();

    public void Dispose() { _context.Database.EnsureDeleted(); _context.Dispose(); }
}

using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.impelementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AngularApi.Tests.Services;

public class PatientReviewServiceTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly PatientReviewService _service;

    public PatientReviewServiceTests()
    {
        _context = new MedicalCenterDbContext(new DbContextOptionsBuilder<MedicalCenterDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        _service = new PatientReviewService(_context, new OwnershipValidator());
    }

    [Fact] public async Task GetAllAsync_ReturnsPagedReviews() { Seed(1, "p1", 5); Seed(2, "p2", 4); (await _service.GetAllAsync(new PaginationParameters())).Items.Should().HaveCount(2); }
    [Fact] public async Task GetUniquePatientsAsync_ReturnsDistinctPatients() { var p = new Patient { Id = "p1", UserName = "John", Email = "j@e.com", Name = "John" }; _context.PatientReviews.AddRange(new PatientReview { Id = 1, PatientId = "p1", Patient = p, DoctorId = "d1", OverallRating = 5 }, new PatientReview { Id = 2, PatientId = "p1", Patient = p, DoctorId = "d2", OverallRating = 4 }); await _context.SaveChangesAsync(); (await _service.GetUniquePatientsAsync(new PaginationParameters())).Items.Should().HaveCount(1); }
    [Fact] public async Task GetByIdAsync_ExistingId_ReturnsReview() { Seed(1, "p1", 5); (await _service.GetByIdAsync(1))!.OverallRating.Should().Be(5); }
    [Fact] public async Task GetByIdAsync_MissingId_ReturnsNull() => (await _service.GetByIdAsync(999)).Should().BeNull();
    [Fact] public async Task CreateAsync_OwnPatient_PersistsReview() { var c = await _service.CreateAsync(new CreatePatientReviewDTO { DoctorId = "d1", OverallRating = 5 }, User("p1", "user")); c!.PatientId.Should().Be("p1"); c.ReviewDate.Should().NotBeNull(); }
    [Fact] public async Task CreateAsync_AdminUser_CanCreateForAnyPatientContext() => (await _service.CreateAsync(new CreatePatientReviewDTO { DoctorId = "d1", OverallRating = 4 }, User("admin", "admin")))!.PatientId.Should().Be("admin");
    [Fact] public async Task CreateAsync_MissingUserId_ReturnsNull() => (await _service.CreateAsync(new CreatePatientReviewDTO { DoctorId = "d1", OverallRating = 5 }, new ClaimsPrincipal(new ClaimsIdentity()))).Should().BeNull();
    [Fact] public async Task UpdateAsync_OwnReview_UpdatesRecord() { Seed(1, "p1", 3); (await _service.UpdateAsync(1, new UpdatePatientReviewDTO { OverallRating = 5, Review = "Updated" }, User("p1", "user"))).Should().BeTrue(); (await _context.PatientReviews.FindAsync(1))!.OverallRating.Should().Be(5); }
    [Fact] public async Task UpdateAsync_OtherPatientReview_Denied() { Seed(1, "p2", 3); (await _service.UpdateAsync(1, new UpdatePatientReviewDTO { OverallRating = 5 }, User("p1", "user"))).Should().BeFalse(); }
    [Fact] public async Task UpdateAsync_MissingReview_ReturnsFalse() => (await _service.UpdateAsync(1, new UpdatePatientReviewDTO { OverallRating = 5 }, User("p1", "user"))).Should().BeFalse();
    [Fact] public async Task DeleteAsync_ExistingId_RemovesReview() { Seed(1, "p1", 5); (await _service.DeleteAsync(1)).Should().BeTrue(); (await _context.PatientReviews.FindAsync(1)).Should().BeNull(); }
    [Fact] public async Task DeleteAsync_MissingId_ReturnsFalse() => (await _service.DeleteAsync(999)).Should().BeFalse();

    void Seed(int id, string pid, int rating) { _context.PatientReviews.Add(new PatientReview { Id = id, PatientId = pid, DoctorId = "d1", OverallRating = rating }); _context.SaveChanges(); }
    static ClaimsPrincipal User(string id, string role) => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id), new Claim(ClaimTypes.Role, role) }, "TestAuth"));
    public void Dispose() { _context.Database.EnsureDeleted(); _context.Dispose(); }
}

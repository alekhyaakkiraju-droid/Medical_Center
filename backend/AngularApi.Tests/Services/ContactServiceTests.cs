using AngularApi.Models;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services;
using AngularApi.Services;
using AngularApi.Services.impelementation;
using AngularApi.Contracts.Services.Interfaces;
using AngularApi.Tests.TestData;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AngularApi.Tests.Services;

public class ContactServiceTests : IDisposable
{
    private readonly MedicalCenterDbContext _context;
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly ContactService _service;

    public ContactServiceTests()
    {
        _context = new MedicalCenterDbContext(
            new DbContextOptionsBuilder<MedicalCenterDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        _service = new ContactService(
            _context,
            _emailServiceMock.Object,
            configuration,
            NullLogger<ContactService>.Instance);
    }

    [Fact]
    public async Task SubmitInquiryAsync_ValidDto_PersistsInquiry()
    {
        var result = await _service.SubmitInquiryAsync(ContactInquiryFixtures.Valid);

        result.Should().BeTrue();
        var saved = await _context.ContactInquiries.SingleAsync();
        saved.Name.Should().Be(ContactInquiryFixtures.Valid.Name);
        saved.Email.Should().Be(ContactInquiryFixtures.Valid.Email);
        saved.Phone.Should().Be(ContactInquiryFixtures.Valid.Phone);
        saved.Message.Should().Be(ContactInquiryFixtures.Valid.Message);
        saved.IsRead.Should().BeFalse();
        saved.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SubmitInquiryAsync_WithAdminEmailConfigured_ForwardsEmail()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ContactSettings:AdminEmail"] = "admin@example.com",
            })
            .Build();

        var service = new ContactService(
            _context,
            _emailServiceMock.Object,
            configuration,
            NullLogger<ContactService>.Instance);

        var result = await service.SubmitInquiryAsync(ContactInquiryFixtures.Valid);

        result.Should().BeTrue();
        _emailServiceMock.Verify(
            emailService => emailService.SendEmailAsync(It.Is<Message>(message =>
                message.Subject == "New Contact Inquiry"
                && message.To.Single() == "admin@example.com")),
            Times.Once);
    }

    [Fact]
    public async Task SubmitInquiryAsync_EmailForwardingFails_StillReturnsTrue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ContactSettings:AdminEmail"] = "admin@example.com",
            })
            .Build();

        _emailServiceMock
            .Setup(emailService => emailService.SendEmailAsync(It.IsAny<Message>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var service = new ContactService(
            _context,
            _emailServiceMock.Object,
            configuration,
            NullLogger<ContactService>.Instance);

        var result = await service.SubmitInquiryAsync(ContactInquiryFixtures.Valid);

        result.Should().BeTrue();
        (await _context.ContactInquiries.CountAsync()).Should().Be(1);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

using AngularApi.Models;
using AngularApi.Contracts.Services;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;

namespace AngularApi.Services.impelementation
{
    public class ContactService : IContactService
    {
        private readonly MedicalCenterDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContactService> _logger;

        public ContactService(
            MedicalCenterDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<ContactService> logger)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SubmitInquiryAsync(ContactInquiryDTO dto, CancellationToken cancellationToken = default)
        {
            var inquiry = new ContactInquiry
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Message = dto.Message,
                CreatedAtUtc = DateTime.UtcNow,
                IsRead = false,
            };

            _context.ContactInquiries.Add(inquiry);
            await _context.SaveChangesAsync(cancellationToken);

            var adminEmail = _configuration["ContactSettings:AdminEmail"];
            if (!string.IsNullOrWhiteSpace(adminEmail))
            {
                try
                {
                    var body = $"""
                        <p><strong>Name:</strong> {System.Net.WebUtility.HtmlEncode(dto.Name)}</p>
                        <p><strong>Email:</strong> {System.Net.WebUtility.HtmlEncode(dto.Email)}</p>
                        <p><strong>Phone:</strong> {System.Net.WebUtility.HtmlEncode(dto.Phone ?? "N/A")}</p>
                        <p><strong>Message:</strong></p>
                        <p>{System.Net.WebUtility.HtmlEncode(dto.Message)}</p>
                        """;

                    await _emailService.SendEmailAsync(
                        new Message(new[] { adminEmail }, "New Contact Inquiry", body));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to forward contact inquiry email for inquiry {InquiryId}", inquiry.Id);
                }
            }

            return true;
        }
    }
}

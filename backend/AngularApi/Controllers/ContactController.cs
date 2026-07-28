using AngularApi.Contracts.DTO;
using AngularApi.Services;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly IRecaptchaService _recaptchaService;

        public ContactController(IContactService contactService, IRecaptchaService recaptchaService)
        {
            _contactService = contactService;
            _recaptchaService = recaptchaService;
        }

        [AllowAnonymous]
        [EnableRateLimiting(AuthRateLimitingExtensions.ContactSubmitPolicy)]
        [HttpPost]
        public async Task<IActionResult> SubmitInquiry([FromBody] ContactInquiryDTO dto, CancellationToken cancellationToken)
        {
            var isHuman = await _recaptchaService.ValidateTokenAsync(dto.RecaptchaToken);
            if (!isHuman)
            {
                return BadRequest(new { error = "reCAPTCHA validation failed" });
            }

            var success = await _contactService.SubmitInquiryAsync(dto, cancellationToken);
            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to submit inquiry." });
            }

            return Ok(new { message = "Your inquiry has been submitted successfully." });
        }
    }
}

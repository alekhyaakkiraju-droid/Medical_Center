using AngularApi.DTO;
using AngularApi.Services;
using AngularApi.Services.Interfaces;
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

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [EnableRateLimiting(AuthRateLimitingExtensions.ContactSubmitPolicy)]
        [HttpPost]
        public async Task<IActionResult> SubmitInquiry([FromBody] ContactInquiryDTO dto, CancellationToken cancellationToken)
        {
            var success = await _contactService.SubmitInquiryAsync(dto, cancellationToken);
            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to submit inquiry." });
            }

            return Ok(new { message = "Your inquiry has been submitted successfully." });
        }
    }
}

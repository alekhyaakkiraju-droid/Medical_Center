using AngularApi.DTO;
using AngularApi.Filters;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminPolicy")]
    public class PatientReviewsController : ControllerBase
    {
        private readonly IPatientReviewService _patientReviewService;

        public PatientReviewsController(IPatientReviewService patientReviewService)
        {
            _patientReviewService = patientReviewService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ReviewDTO>>> GetPatientReviews([FromQuery] PaginationParameters pagination) =>
            await _patientReviewService.GetAllAsync(pagination);

        [HttpGet("unique-patients")]
        public async Task<ActionResult<PagedResult<PatientDTO>>> GetUniquePatients([FromQuery] PaginationParameters pagination) =>
            await _patientReviewService.GetUniquePatientsAsync(pagination);

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientReview>> GetPatientReview(int id)
        {
            var patientReview = await _patientReviewService.GetByIdAsync(id);
            return patientReview == null ? NotFound() : patientReview;
        }

        [HttpPut("{id}")]
        [ValidateOwnership(ResourceType.PatientReview)]
        public async Task<IActionResult> PutPatientReview(int id, [FromBody] UpdatePatientReviewDTO dto) =>
            await _patientReviewService.UpdateAsync(id, dto, User) ? NoContent() : NotFound();

        [HttpPost]
        public async Task<ActionResult<PatientReview>> PostPatientReview([FromBody] CreatePatientReviewDTO dto)
        {
            var created = await _patientReviewService.CreateAsync(dto, User);
            return created == null ? BadRequest() : CreatedAtAction(nameof(GetPatientReview), new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        [ValidateOwnership(ResourceType.PatientReview)]
        public async Task<IActionResult> DeletePatientReview(int id) =>
            await _patientReviewService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

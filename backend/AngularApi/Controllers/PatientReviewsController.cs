using AngularApi.DTO; using AngularApi.Models; using AngularApi.Services.Interfaces; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc;
namespace AngularApi.Controllers {
  [Route("api/[controller]")][ApiController][Authorize(Policy="AdminPolicy")]
  public class PatientReviewsController : ControllerBase {
    private readonly IPatientReviewService _svc; public PatientReviewsController(IPatientReviewService svc)=>_svc=svc;
    [HttpGet] public async Task<ActionResult<PagedResult<ReviewDTO>>> GetPatientReviews([FromQuery] PaginationParameters p)=>await _svc.GetAllAsync(p);
    [HttpGet("unique-patients")] public async Task<ActionResult<PagedResult<PatientDTO>>> GetUniquePatients([FromQuery] PaginationParameters p)=>await _svc.GetUniquePatientsAsync(p);
    [HttpGet("{id}")] public async Task<ActionResult<PatientReview>> GetPatientReview(int id){var r=await _svc.GetByIdAsync(id); return r==null?NotFound():Ok(r);}
    [HttpPut("{id}")] public async Task<IActionResult> PutPatientReview(int id,[FromBody] UpdatePatientReviewDTO dto)=>await _svc.UpdateAsync(id,dto,User)?NoContent():NotFound();
    [HttpPost] public async Task<ActionResult<PatientReview>> PostPatientReview([FromBody] CreatePatientReviewDTO dto){var c=await _svc.CreateAsync(dto,User); return c==null?BadRequest():CreatedAtAction(nameof(GetPatientReview),new{id=c.Id},c);}
    [HttpDelete("{id}")] public async Task<IActionResult> DeletePatientReview(int id)=>await _svc.DeleteAsync(id)?NoContent():NotFound();
  }
}

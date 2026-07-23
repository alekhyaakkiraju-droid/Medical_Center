using AngularApi.DTO;
using AngularApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminPolicy")]
    public class PatientReviewsController : ControllerBase
    {
        private readonly MedicalCenterDbContext _context;

        public PatientReviewsController(MedicalCenterDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetPatientReviews()
        {
            var reviews = await _context.PatientReviews
                .Select(r => new ReviewDTO
                {
                    Id = r.Id,
                    PatientId = r.PatientId,
                    DoctorId = r.DoctorId,
                    IsReviewAnonymous = r.IsReviewAnonymous,
                    WaitTimeRating = r.WaitTimeRating,
                    BedsideMannerRating = r.BedsideMannerRating,
                    OverallRating = r.OverallRating,
                    Review = r.Review,
                    IsDoctorRecommended = r.IsDoctorRecommended,
                    ReviewDate = r.ReviewDate
                })
                .ToListAsync();
            return Ok(reviews);
        }

        [HttpGet("unique-patients")]
        public async Task<ActionResult<IEnumerable<PatientDTO>>> GetUniquePatients()
        {
            var uniquePatients = await _context.PatientReviews
                .Where(pr => pr.Patient != null)
                .Select(pr => new PatientDTO
                {
                    PatientId = pr.Patient!.Id,
                    Name = pr.Patient.UserName,
                    Email = pr.Patient.Email,
                    Image = pr.Patient.Image
                })
                .Distinct()
                .ToListAsync();

            return Ok(uniquePatients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientReview>> GetPatientReview(int id)
        {
            var patientReview = await _context.PatientReviews.FindAsync(id);

            if (patientReview == null)
            {
                return NotFound();
            }

            return Ok(patientReview);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutPatientReview(int id, PatientReview patientReview)
        {
            if (id != patientReview.Id)
            {
                return BadRequest();
            }

            _context.Entry(patientReview).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientReviewExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpPost]
        public async Task<ActionResult<PatientReview>> PostPatientReview(PatientReview patientReview)
        {
            _context.PatientReviews.Add(patientReview);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPatientReview", new { id = patientReview.Id }, patientReview);
        }

        // DELETE: api/PatientReviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatientReview(int id)
        {
            var patientReview = await _context.PatientReviews.FindAsync(id);
            if (patientReview == null)
            {
                return NotFound();
            }

            _context.PatientReviews.Remove(patientReview);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PatientReviewExists(int id)
        {
            return _context.PatientReviews.Any(e => e.Id == id);
        }
    }
}

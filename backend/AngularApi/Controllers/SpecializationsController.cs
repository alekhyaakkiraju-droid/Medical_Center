using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminPolicy")]
    public class SpecializationsController : ControllerBase
    {
        private readonly ISpecializationService _specializationService;
        public SpecializationsController(ISpecializationService specializationService) => _specializationService = specializationService;

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<PagedResult<SpecializationListItemDTO>>> GetSpecializations([FromQuery] PaginationParameters pagination) =>
            await _specializationService.GetSpecializationsAsync(pagination);

        [HttpGet("{id}")]
        public async Task<ActionResult<SpecializationDetailDTO>> GetSpecialization(int id)
        {
            var specialization = await _specializationService.GetSpecializationByIdAsync(id);
            return specialization == null ? NotFound() : specialization;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpecialization(int id, [FromBody] UpdateSpecializationDTO dto)
        {
            return await _specializationService.UpdateSpecializationAsync(id, dto) ? NoContent() : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Specialization>> PostSpecialization([FromBody] CreateSpecializationDTO dto)
        {
            var created = await _specializationService.CreateSpecializationAsync(dto);
            return CreatedAtAction(nameof(GetSpecialization), new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpecialization(int id) =>
            await _specializationService.DeleteSpecializationAsync(id) ? NoContent() : NotFound();
    }
}

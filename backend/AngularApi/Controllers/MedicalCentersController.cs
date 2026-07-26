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
    public class MedicalCentersController : ControllerBase
    {
        private readonly IMedicalCenterService _medicalCenterService;

        public MedicalCentersController(IMedicalCenterService medicalCenterService)
        {
            _medicalCenterService = medicalCenterService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<MedicalCenterListItemDTO>>> GetMedicalCenter(
            [FromQuery] PaginationParameters pagination)
        {
            return await _medicalCenterService.GetMedicalCentersAsync(pagination);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalCenterDetailDTO>> GetMedicalCenter(int id)
        {
            var medicalCenter = await _medicalCenterService.GetMedicalCenterByIdAsync(id);
            return medicalCenter != null ? medicalCenter : NotFound();
        }

        [HttpPut("{id}")]
        [ValidateOwnership(ResourceType.MedicalCenter)]
        public async Task<IActionResult> PutMedicalCenter(int id, UpdateMedicalCenterDTO dto)
        {
            var updated = await _medicalCenterService.UpdateMedicalCenterAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<MedicalCenter>> PostMedicalCenter(CreateMedicalCenterDTO dto)
        {
            var created = await _medicalCenterService.CreateMedicalCenterAsync(dto);
            return CreatedAtAction("GetMedicalCenter", new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        [ValidateOwnership(ResourceType.MedicalCenter)]
        public async Task<IActionResult> DeleteMedicalCenter(int id)
        {
            var deleted = await _medicalCenterService.DeleteMedicalCenterAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

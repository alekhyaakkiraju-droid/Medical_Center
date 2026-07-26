using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminPolicy")]
    public class AppointmentStatusController : ControllerBase
    {
        private readonly IAppointmentStatusService _appointmentStatusService;

        public AppointmentStatusController(IAppointmentStatusService appointmentStatusService)
        {
            _appointmentStatusService = appointmentStatusService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<AppointmentStatusListItemDTO>>> GetAppointmentStatus([FromQuery] PaginationParameters pagination)
        {
            return await _appointmentStatusService.GetAllAsync(pagination);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentStatus>> GetAppointmentStatus(int id)
        {
            var appointmentStatus = await _appointmentStatusService.GetByIdAsync(id);

            if (appointmentStatus == null)
            {
                return NotFound();
            }

            return appointmentStatus;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppointmentStatus(int id, [FromBody] UpdateAppointmentStatusDTO dto)
        {
            var updated = await _appointmentStatusService.UpdateAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentStatus>> PostAppointmentStatus([FromBody] CreateAppointmentStatusDTO dto)
        {
            var created = await _appointmentStatusService.CreateAsync(dto);
            return CreatedAtAction("GetAppointmentStatus", new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointmentStatus(int id)
        {
            var deleted = await _appointmentStatusService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

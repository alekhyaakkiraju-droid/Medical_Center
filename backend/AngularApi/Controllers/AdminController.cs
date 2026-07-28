using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularApi.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Policy = "AdminPolicy")]
    public class AdminController : ControllerBase
    {
        private readonly IBreachNotificationService _breachNotificationService;

        public AdminController(IBreachNotificationService breachNotificationService)
        {
            _breachNotificationService = breachNotificationService;
        }

        [HttpPost("breach-assessment")]
        public async Task<ActionResult<BreachAssessmentResultDTO>> AssessBreach(
            [FromBody] BreachAssessmentDTO assessment,
            CancellationToken cancellationToken)
        {
            var result = await _breachNotificationService.AssessBreachAsync(assessment, cancellationToken);
            return Ok(result);
        }
    }
}

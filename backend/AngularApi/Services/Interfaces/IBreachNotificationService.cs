using AngularApi.DTO;

namespace AngularApi.Services.Interfaces
{
    public interface IBreachNotificationService
    {
        Task<IReadOnlyList<BreachAnomalyDTO>> DetectAnomaliesAsync(CancellationToken cancellationToken = default);
        Task<BreachAssessmentResultDTO> AssessBreachAsync(BreachAssessmentDTO assessment, CancellationToken cancellationToken = default);
        Task<BreachAssessmentResultDTO> NotifyAffectedIndividualsAsync(
            Guid assessmentId,
            BreachAssessmentDTO assessment,
            CancellationToken cancellationToken = default);
    }
}

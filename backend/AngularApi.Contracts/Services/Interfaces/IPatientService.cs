using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;

namespace AngularApi.Contracts.Services.Interfaces;

public interface IPatientService
{
    Task<PagedResult<PatientDTO>> GetAllPatientsWithReviewsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PatientDTO?> GetPatientByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDTO>> GetPatientAppointmentsAsync(string patientId, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDTO>> GetAppointmentsByDateRangeAsync(string patientId, DateTime startDate, DateTime endDate, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task UpdateReviewAsync(PatientReview review, CancellationToken cancellationToken = default);

    Task<bool> DeletePatientAppointmentAsync(string patientId, int appointmentId, CancellationToken cancellationToken = default);

    Task<bool> UpdatePatientAsync(string id, PatientDTO model, CancellationToken cancellationToken = default);

    Task<bool> DeletePatientAsync(string id, CancellationToken cancellationToken = default);
}

using AngularApi.Contracts.Enums;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;

namespace AngularApi.Contracts.Services.Interfaces;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentDTO>> GetAppointmentsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDTO>> GetAllAppointmentsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<Appointment?> GetAppointmentByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> UpdateAppointmentAsync(int id, UpdateAppointmentDTO appointmentDto, CancellationToken cancellationToken = default);

    Task<decimal> GetTotalEarningsAsync(CancellationToken cancellationToken = default);

    Task<(Appointment? Appointment, string? ErrorMessage)> CreateAppointmentAsync(CreateAppointmentDTO dto, string patientId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAppointmentAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDTO>> GetAppointmentsByPatientAsync(string patientId, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDTO>> GetAppointmentsByDateAsync(DateTime date, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDTO>> GetAppointmentsByStatusAsync(AppointmentStatusEnum status, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDTO>> GetTodaysAppointmentsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDTO>> GetUpcomingAppointmentsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDTO>> GetAppointmentsByPatientAndStatusAsync(string patientId, AppointmentStatusEnum status, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<AppointmentDTO>> GetAppointmentHistoryByPatientAsync(string patientId, PaginationParameters pagination, CancellationToken cancellationToken = default);
}

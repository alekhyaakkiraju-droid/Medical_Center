using AngularApi.Contracts.Enums;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;

namespace AngularApi.Contracts.Services.Interfaces;

public interface IDoctorService
{
    Task<PagedResult<DoctorDTO>> GetDoctorsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<DoctorDTO>> GetDoctorsWithSpecializationAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<DoctorDetailDTO?> GetDoctorByIdAsync(string doctorId, CancellationToken cancellationToken = default);

    Task<Doctor> CreateDoctorAsync(Doctor doctor, CancellationToken cancellationToken = default);

    Task<PagedResult<BookingDTO>> GetBookingsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<BookingDTO>> GetBookingsByStatusAsync(string doctorId, AppointmentStatusEnum status, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<BookingDTO>> GetTodaysBookingsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<BookingDTO>> GetUpcomingBookingsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<BookingDTO>> GetLast30DaysBookingsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<ReviewDTO>> GetReviewsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<double> GetRatingAsync(string doctorId, CancellationToken cancellationToken = default);

    Task<PagedResult<DoctorQualificationDTO>> GetQualificationsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<PagedResult<string>> GetSpecializationsAsync(string doctorId, PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<bool> UpdateDoctorAsync(string id, Doctor doctor, CancellationToken cancellationToken = default);

    Task<bool> UpdateBookingAsync(int bookingId, Appointment updatedBooking, CancellationToken cancellationToken = default);

    Task<bool> DeleteDoctorAsync(string id, CancellationToken cancellationToken = default);

    Task<bool> CancelDoctorAppointmentAsync(string doctorId, int appointmentId, CancellationToken cancellationToken = default);
}

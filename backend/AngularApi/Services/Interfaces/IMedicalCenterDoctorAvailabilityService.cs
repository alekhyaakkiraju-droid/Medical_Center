using AngularApi.DTO;
using AngularApi.Models;

namespace AngularApi.Services.Interfaces;

public interface IMedicalCenterDoctorAvailabilityService
{
    Task<PagedResult<MedicalCenterDoctorAvailabilityDTO>> GetAllAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<MedicalCenterDoctorAvailability?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<MedicalCenterDoctorAvailability?> CreateAsync(CreateMedicalCenterDoctorAvailabilityDTO dto, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(int id, UpdateMedicalCenterDoctorAvailabilityDTO dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

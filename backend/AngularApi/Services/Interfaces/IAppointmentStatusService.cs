using AngularApi.DTO;
using AngularApi.Models;

namespace AngularApi.Services.Interfaces;

public interface IAppointmentStatusService
{
    Task<PagedResult<AppointmentStatusListItemDTO>> GetAllAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);

    Task<AppointmentStatusDetailDTO?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<AppointmentStatus> CreateAsync(CreateAppointmentStatusDTO dto, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(int id, UpdateAppointmentStatusDTO dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

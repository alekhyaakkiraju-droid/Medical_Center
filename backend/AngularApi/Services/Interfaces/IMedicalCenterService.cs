using AngularApi.DTO;
using AngularApi.Models;

namespace AngularApi.Services.Interfaces;

public interface IMedicalCenterService
{
    Task<PagedResult<MedicalCenterListItemDTO>> GetMedicalCentersAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken = default);

    Task<MedicalCenterDetailDTO?> GetMedicalCenterByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<MedicalCenter> CreateMedicalCenterAsync(
        CreateMedicalCenterDTO dto,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateMedicalCenterAsync(
        int id,
        UpdateMedicalCenterDTO dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteMedicalCenterAsync(int id, CancellationToken cancellationToken = default);
}

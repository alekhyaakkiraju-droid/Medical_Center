using AngularApi.DTO;
using AngularApi.Models;

namespace AngularApi.Services.Interfaces;

public interface IMedicalCenterService
{
    Task<PagedResult<MedicalCenterListItemDTO>> GetMedicalCentersAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken = default);

    Task<MedicalCenter?> GetMedicalCenterByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<MedicalCenter> CreateMedicalCenterAsync(
        MedicalCenter medicalCenter,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateMedicalCenterAsync(
        int id,
        MedicalCenter medicalCenter,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteMedicalCenterAsync(int id, CancellationToken cancellationToken = default);
}

using AngularApi.DTO; using AngularApi.Models; using System.Security.Claims;
namespace AngularApi.Services.Interfaces;
public interface IMedicalCenterService {
    Task<PagedResult<MedicalCenterListItemDTO>> GetMedicalCentersAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);
    Task<MedicalCenter?> GetMedicalCenterByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(MedicalCenter? Center, ResourceMutationResult Result)> CreateMedicalCenterAsync(CreateMedicalCenterDTO dto, ClaimsPrincipal user, CancellationToken cancellationToken = default);
    Task<ResourceMutationResult> UpdateMedicalCenterAsync(int id, UpdateMedicalCenterDTO dto, ClaimsPrincipal user, CancellationToken cancellationToken = default);
    Task<ResourceMutationResult> DeleteMedicalCenterAsync(int id, ClaimsPrincipal user, CancellationToken cancellationToken = default);
}

using AngularApi.DTO; using AngularApi.Models; using System.Security.Claims;
namespace AngularApi.Services.Interfaces;
public interface IPatientReviewService {
    Task<PagedResult<ReviewDTO>> GetAllAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);
    Task<PagedResult<PatientDTO>> GetUniquePatientsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);
    Task<PatientReview?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PatientReview?> CreateAsync(CreatePatientReviewDTO dto, ClaimsPrincipal user, CancellationToken cancellationToken = default);
    Task<ResourceMutationResult> UpdateAsync(int id, UpdatePatientReviewDTO dto, ClaimsPrincipal user, CancellationToken cancellationToken = default);
    Task<ResourceMutationResult> DeleteAsync(int id, ClaimsPrincipal user, CancellationToken cancellationToken = default);
}

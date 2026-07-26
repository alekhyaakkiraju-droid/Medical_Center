using AngularApi.DTO;
using AngularApi.Models;

namespace AngularApi.Services.Interfaces;

public interface ISpecializationService
{
    Task<PagedResult<SpecializationListItemDTO>> GetSpecializationsAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);
    Task<Specialization?> GetSpecializationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Specialization> CreateSpecializationAsync(Specialization specialization, CancellationToken cancellationToken = default);
    Task<bool> UpdateSpecializationAsync(int id, Specialization specialization, CancellationToken cancellationToken = default);
    Task<bool> DeleteSpecializationAsync(int id, CancellationToken cancellationToken = default);
}

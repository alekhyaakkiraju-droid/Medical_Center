using AngularApi.Contracts.DTO;

namespace AngularApi.Contracts.Services.Interfaces;

public interface INppService
{
    Task<NppStatusResponse> GetStatusAsync(string userId, CancellationToken cancellationToken = default);

    Task AcknowledgeAsync(string userId, CancellationToken cancellationToken = default);

    Task<NppContentResponse> GetContentAsync(CancellationToken cancellationToken = default);
}

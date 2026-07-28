namespace AngularApi.Contracts.Services.Interfaces
{
    public interface IAuditService
    {
        Task RecordAsync(
            string action,
            string? entityType = null,
            string? entityId = null,
            string? oldValues = null,
            string? newValues = null,
            string? actor = null,
            CancellationToken cancellationToken = default);

        Task RecordAuthEventAsync(
            string action,
            string? actorIdentifier,
            bool succeeded,
            CancellationToken cancellationToken = default);
    }
}

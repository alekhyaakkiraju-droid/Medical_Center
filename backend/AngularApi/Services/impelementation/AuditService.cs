using AngularApi.Models;
using AngularApi.Services.Interfaces;
using System.Security.Claims;

namespace AngularApi.Services.impelementation
{
    public class AuditService : IAuditService
    {
        private readonly MedicalCenterDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(MedicalCenterDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RecordAsync(
            string action,
            string? entityType = null,
            string? entityId = null,
            string? oldValues = null,
            string? newValues = null,
            string? actor = null,
            CancellationToken cancellationToken = default)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Actor = actor ?? ResolveActor(),
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task RecordAuthEventAsync(
            string action,
            string? actorIdentifier,
            bool succeeded,
            CancellationToken cancellationToken = default)
        {
            var status = succeeded ? "Succeeded" : "Failed";
            return RecordAsync(
                action,
                entityType: "Authentication",
                entityId: actorIdentifier,
                newValues: status,
                actor: actorIdentifier ?? "anonymous",
                cancellationToken: cancellationToken);
        }

        private string ResolveActor()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return "anonymous";
            }

            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(ClaimTypes.Email)
                ?? user.Identity?.Name
                ?? "anonymous";
        }
    }
}

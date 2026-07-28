using System.Text.Json;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Models;
using AngularApi.Contracts.Services.Interfaces;
using AngularApi.Models;
using AngularApi.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AngularApi.Services.impelementation;

public class NppService : INppService
{
    private readonly MedicalCenterDbContext _context;
    private readonly NppSettings _settings;
    private readonly IWebHostEnvironment _environment;

    public NppService(
        MedicalCenterDbContext context,
        IOptions<NppSettings> settings,
        IWebHostEnvironment environment)
    {
        _context = context;
        _settings = settings.Value;
        _environment = environment;
    }

    public async Task<NppStatusResponse> GetStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        var latestAcknowledgment = await _context.AuditLogs
            .Where(log => log.EntityType == "NPPAcknowledgment" && log.Actor == userId)
            .OrderByDescending(log => log.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestAcknowledgment is null)
        {
            return new NppStatusResponse
            {
                Acknowledged = false,
                Version = _settings.CurrentVersion,
            };
        }

        var acknowledgedVersion = ExtractVersion(latestAcknowledgment.NewValues);
        var acknowledged = acknowledgedVersion == _settings.CurrentVersion;

        return new NppStatusResponse
        {
            Acknowledged = acknowledged,
            AcknowledgedAt = latestAcknowledgment.Timestamp,
            Version = _settings.CurrentVersion,
        };
    }

    public async Task AcknowledgeAsync(string userId, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            version = _settings.CurrentVersion,
            timestamp = DateTime.UtcNow,
        });

        _context.AuditLogs.Add(new AuditLog
        {
            Actor = userId,
            Action = "Acknowledge",
            EntityType = "NPPAcknowledgment",
            EntityId = userId,
            NewValues = payload,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<NppContentResponse> GetContentAsync(CancellationToken cancellationToken = default)
    {
        var contentPath = Path.Combine(_environment.ContentRootPath, _settings.ContentFilePath);
        var content = await File.ReadAllTextAsync(contentPath, cancellationToken);
        var lastUpdated = File.GetLastWriteTimeUtc(contentPath).ToString("O");

        return new NppContentResponse
        {
            Content = content,
            Version = _settings.CurrentVersion,
            LastUpdated = lastUpdated,
        };
    }

    private static string? ExtractVersion(string? newValues)
    {
        if (string.IsNullOrWhiteSpace(newValues))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(newValues);
            return document.RootElement.TryGetProperty("version", out var versionElement)
                ? versionElement.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

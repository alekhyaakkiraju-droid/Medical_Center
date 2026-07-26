using System.Text.Json;
using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Options;
using AngularApi.Services;
using AngularApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AngularApi.Services.impelementation
{
    public class BreachNotificationService : IBreachNotificationService
    {
        private readonly MedicalCenterDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly BreachDetectionOptions _options;

        public BreachNotificationService(
            MedicalCenterDbContext context,
            IAuditService auditService,
            IEmailService emailService,
            EmailTemplateService emailTemplateService,
            IOptions<BreachDetectionOptions> options)
        {
            _context = context;
            _auditService = auditService;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _options = options.Value;
        }

        public async Task<IReadOnlyList<BreachAnomalyDTO>> DetectAnomaliesAsync(CancellationToken cancellationToken = default)
        {
            var windowEnd = DateTime.UtcNow;
            var windowStart = windowEnd.AddMinutes(-_options.WindowMinutes);
            var anomalies = new List<BreachAnomalyDTO>();

            var failedAuthGroups = await _context.AuditLogs
                .AsNoTracking()
                .Where(log =>
                    log.Timestamp >= windowStart
                    && log.Timestamp <= windowEnd
                    && log.EntityType == "Authentication"
                    && log.NewValues == "Failed")
                .GroupBy(log => log.Actor)
                .Select(group => new { Actor = group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken);

            foreach (var group in failedAuthGroups.Where(g => g.Count >= _options.FailedAuthThreshold))
            {
                anomalies.Add(new BreachAnomalyDTO
                {
                    AnomalyType = "FailedAuthenticationSpike",
                    Actor = group.Actor,
                    EventCount = group.Count,
                    WindowStart = windowStart,
                    WindowEnd = windowEnd,
                    Description =
                        $"Actor '{group.Actor}' recorded {group.Count} failed authentication attempts within {_options.WindowMinutes} minutes."
                });
            }

            var mutationGroups = await _context.AuditLogs
                .AsNoTracking()
                .Where(log =>
                    log.Timestamp >= windowStart
                    && log.Timestamp <= windowEnd
                    && log.EntityType != null
                    && log.EntityType != "Authentication"
                    && log.EntityType != "BreachAssessment")
                .GroupBy(log => log.Actor)
                .Select(group => new { Actor = group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken);

            foreach (var group in mutationGroups.Where(g => g.Count >= _options.MutationThreshold))
            {
                anomalies.Add(new BreachAnomalyDTO
                {
                    AnomalyType = "UnusualDataAccessVolume",
                    Actor = group.Actor,
                    EventCount = group.Count,
                    WindowStart = windowStart,
                    WindowEnd = windowEnd,
                    Description =
                        $"Actor '{group.Actor}' performed {group.Count} data mutations within {_options.WindowMinutes} minutes."
                });
            }

            return anomalies;
        }

        public async Task<BreachAssessmentResultDTO> AssessBreachAsync(
            BreachAssessmentDTO assessment,
            CancellationToken cancellationToken = default)
        {
            var assessmentId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                assessmentId,
                assessment.Description,
                assessment.AffectedEntityTypes,
                assessment.DiscoveryDate,
                assessment.SeverityLevel,
                assessment.AffectedIndividualEmails
            });

            await _auditService.RecordAsync(
                "BreachAssessment",
                entityType: "BreachAssessment",
                entityId: assessmentId.ToString(),
                newValues: payload,
                cancellationToken: cancellationToken);

            if (assessment.AffectedIndividualEmails.Count == 0)
            {
                return new BreachAssessmentResultDTO
                {
                    AssessmentId = assessmentId,
                    Status = "Assessed",
                    NotificationsSent = 0,
                    NotificationsFailed = 0
                };
            }

            var notificationResult = await NotifyAffectedIndividualsAsync(assessmentId, assessment, cancellationToken);
            notificationResult.Status = "AssessedWithNotifications";
            return notificationResult;
        }

        public async Task<BreachAssessmentResultDTO> NotifyAffectedIndividualsAsync(
            Guid assessmentId,
            BreachAssessmentDTO assessment,
            CancellationToken cancellationToken = default)
        {
            var notificationsSent = 0;
            var notificationsFailed = 0;
            var contactEmail = "privacy@medicalcenter.example";

            foreach (var email in assessment.AffectedIndividualEmails.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    var body = _emailTemplateService.GetBreachNotificationEmail(
                        affectedIndividualName: email,
                        breachDescription: assessment.Description,
                        dateDiscovered: assessment.DiscoveryDate.ToString("yyyy-MM-dd"),
                        recommendedActions:
                            "Review your account activity, change your password, and contact our privacy team if you notice unauthorized access.",
                        contactInformation: contactEmail);

                    await _emailService.SendEmailAsync(new Message(
                        [email],
                        "Important Security Notice Regarding Your Protected Health Information",
                        body));

                    notificationsSent++;
                }
                catch
                {
                    notificationsFailed++;
                }
            }

            return new BreachAssessmentResultDTO
            {
                AssessmentId = assessmentId,
                Status = notificationsFailed > 0 ? "NotificationsPartiallySent" : "NotificationsSent",
                NotificationsSent = notificationsSent,
                NotificationsFailed = notificationsFailed
            };
        }
    }
}

using System.Collections.Concurrent;

namespace AngularApi.Services
{
    public class EmailTemplateService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ConcurrentDictionary<string, string> _templateCache = new();

        public EmailTemplateService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string GetConfirmationEmail(string userName, string confirmationLink)
        {
            var emailTemplate = LoadTemplate("ConfirmationEmail.html");

            return emailTemplate
                .Replace("{{UserName}}", userName)
                .Replace("{{ConfirmationLink}}", confirmationLink);
        }

        public string GetAppointmentConfirmationEmail(string patientName, string doctorName, string date)
        {
            var emailTemplate = LoadTemplate("ConfirmAppointment.html");

            return emailTemplate
                .Replace("{{patientName}}", patientName)
                .Replace("{{DoctorName}}", doctorName)
                .Replace("{{date}}", date);
        }


        public string GetBreachNotificationEmail(
            string affectedIndividualName,
            string breachDescription,
            string dateDiscovered,
            string recommendedActions,
            string contactInformation)
        {
            var emailTemplate = LoadTemplate("BreachNotification.html");

            return emailTemplate
                .Replace("{{AffectedIndividualName}}", affectedIndividualName)
                .Replace("{{BreachDescription}}", breachDescription)
                .Replace("{{DateDiscovered}}", dateDiscovered)
                .Replace("{{RecommendedActions}}", recommendedActions)
                .Replace("{{ContactInformation}}", contactInformation);
        }

        private string LoadTemplate(string templateFileName)
        {
            return _templateCache.GetOrAdd(templateFileName, fileName =>
            {
                var templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "EmailTemplates", fileName);
                return File.ReadAllText(templatePath);
            });
        }
    }
}

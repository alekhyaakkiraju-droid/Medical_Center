namespace AngularApi.Options
{
    public class SmtpSettings
    {
        public const string SectionName = "SmtpSettings";

        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool UseTls { get; set; } = true;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && Port > 0;

        public bool IsDevMode =>
            Host.Contains("mailhog", StringComparison.OrdinalIgnoreCase) || Port == 1025;
    }
}

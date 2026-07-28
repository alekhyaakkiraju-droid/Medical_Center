namespace AngularApi.Options
{
    public class RecaptchaSettings
    {
        public const string SectionName = "RecaptchaSettings";

        public string SiteKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public double MinimumScore { get; set; } = 0.5;
        public bool Enabled { get; set; } = true;
    }
}

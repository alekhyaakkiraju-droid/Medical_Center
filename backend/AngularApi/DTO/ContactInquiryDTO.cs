namespace AngularApi.DTO
{
    public class ContactInquiryDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RecaptchaToken { get; set; } = string.Empty;
    }
}

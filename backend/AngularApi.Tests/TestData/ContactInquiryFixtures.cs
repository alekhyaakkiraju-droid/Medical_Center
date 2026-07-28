using AngularApi.DTO;
using AngularApi.Tests.Fixtures.Recaptcha;

namespace AngularApi.Tests.TestData;

public static class ContactInquiryFixtures
{
    public static ContactInquiryDTO Valid => new()
    {
        Name = "Jane Doe",
        Email = "jane.doe@example.com",
        Phone = "5551234567",
        Message = "I would like to schedule an appointment.",
        RecaptchaToken = RecaptchaTokenFixtures.Valid,
    };

    public static ContactInquiryDTO ValidWithoutPhone => new()
    {
        Name = "John Smith",
        Email = "john.smith@example.com",
        Message = "Please contact me about your services.",
        RecaptchaToken = RecaptchaTokenFixtures.Valid,
    };

    public static ContactInquiryDTO MissingName => new()
    {
        Name = "",
        Email = "jane.doe@example.com",
        Message = "Hello",
    };

    public static ContactInquiryDTO InvalidEmail => new()
    {
        Name = "Jane Doe",
        Email = "not-an-email",
        Message = "Hello",
    };

    public static ContactInquiryDTO InvalidName => new()
    {
        Name = "Jane123",
        Email = "jane.doe@example.com",
        Message = "Hello",
    };

    public static ContactInquiryDTO InvalidPhone => new()
    {
        Name = "Jane Doe",
        Email = "jane.doe@example.com",
        Phone = "123",
        Message = "Hello",
    };

    public static ContactInquiryDTO MessageTooLong => new()
    {
        Name = "Jane Doe",
        Email = "jane.doe@example.com",
        Message = new string('x', 2001),
        RecaptchaToken = RecaptchaTokenFixtures.Valid,
    };

    public static ContactInquiryDTO MissingRecaptchaToken => new()
    {
        Name = "Jane Doe",
        Email = "jane.doe@example.com",
        Message = "Hello",
        RecaptchaToken = RecaptchaTokenFixtures.Missing,
    };

    public static ContactInquiryDTO InvalidRecaptchaToken => new()
    {
        Name = "Jane Doe",
        Email = "jane.doe@example.com",
        Message = "Hello",
        RecaptchaToken = RecaptchaTokenFixtures.Invalid,
    };
}

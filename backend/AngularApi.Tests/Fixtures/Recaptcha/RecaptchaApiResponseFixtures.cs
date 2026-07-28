namespace AngularApi.Tests.Fixtures.Recaptcha;

public static class RecaptchaApiResponseFixtures
{
    public const string SuccessfulHighScore = """
        {
          "success": true,
          "score": 0.9,
          "action": "contact_submit",
          "challenge_ts": "2026-07-27T12:00:00Z",
          "hostname": "localhost"
        }
        """;

    public const string SuccessfulLowScore = """
        {
          "success": true,
          "score": 0.2,
          "action": "contact_submit",
          "challenge_ts": "2026-07-27T12:00:00Z",
          "hostname": "localhost"
        }
        """;

    public const string InvalidToken = """
        {
          "success": false,
          "error-codes": ["invalid-input-response"]
        }
        """;

    public const string ApiError = """
        {
          "success": false,
          "error-codes": ["missing-input-secret"]
        }
        """;
}

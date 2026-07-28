namespace AngularApi.Tests.TestData;

public static class AppointmentTestPayloads
{
    public static object Valid(string doctorId, int daysFromNow = 2) => new
    {
        doctorId,
        medicalCenterId = 1,
        appointmentTakenDate = DateTime.UtcNow.AddDays(daysFromNow),
        probableStartTime = DateTime.UtcNow.AddDays(daysFromNow).AddHours(1),
        name = "Test Patient",
        email = "patient@example.com",
        phone = "5551234567",
    };
}

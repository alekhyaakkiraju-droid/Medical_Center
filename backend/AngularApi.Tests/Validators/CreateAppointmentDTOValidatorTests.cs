using AngularApi.Contracts.DTO;
using AngularApi.Validators;
using FluentAssertions;

namespace AngularApi.Tests.Validators;

public class CreateAppointmentDTOValidatorTests
{
    private readonly CreateAppointmentDTOValidator _validator = new();

    private static CreateAppointmentDTO ValidDto() => new()
    {
        DoctorId = "doctor-1",
        MedicalCenterId = 1,
        AppointmentTakenDate = DateTime.UtcNow.AddDays(1),
        ProbableStartTime = DateTime.UtcNow.AddDays(1).AddHours(10),
        Name = "Jane Patient",
        Email = "jane@example.com",
        Phone = "5551234567",
    };

    [Fact]
    public void Validate_ValidDto_Passes()
    {
        var result = _validator.Validate(ValidDto());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MissingDoctorId_Fails()
    {
        var dto = ValidDto();
        dto.DoctorId = string.Empty;

        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAppointmentDTO.DoctorId));
    }

    [Fact]
    public void Validate_InvalidEmail_Fails()
    {
        var dto = ValidDto();
        dto.Email = "not-an-email";

        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAppointmentDTO.Email));
    }

    [Fact]
    public void Validate_PastAppointmentDate_Fails()
    {
        var dto = ValidDto();
        dto.AppointmentTakenDate = DateTime.UtcNow.AddDays(-1);

        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAppointmentDTO.AppointmentTakenDate));
    }
}

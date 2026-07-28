using AngularApi.Contracts.DTO;
using AngularApi.Validators;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.Validators;

public class AppointmentDTOValidatorTests
{
    private readonly AppointmentDTOValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_Passes()
    {
        var dto = new AppointmentDTO
        {
            Doctor = new DoctorDTO { Name = "Dr Smith" },
            AppointmentDate = DateTime.UtcNow.AddDays(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_MissingDoctorName_Fails()
    {
        var dto = new AppointmentDTO
        {
            Doctor = new DoctorDTO { Name = "" },
            AppointmentDate = DateTime.UtcNow.AddDays(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Doctor!.Name);
    }

    [Fact]
    public void Validate_PastAppointmentDate_Fails()
    {
        var dto = new AppointmentDTO
        {
            Doctor = new DoctorDTO { Name = "Dr Smith" },
            AppointmentDate = DateTime.UtcNow.AddDays(-1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AppointmentDate);
    }
}

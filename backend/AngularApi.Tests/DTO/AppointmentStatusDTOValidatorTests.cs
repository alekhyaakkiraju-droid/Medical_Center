using AngularApi.DTO;
using AngularApi.Models;
using FluentValidation.TestHelper;

namespace AngularApi.Tests.DTO;

public class AppointmentStatusDTOValidatorTests
{
    private readonly CreateAppointmentStatusDTOValidator _createValidator = new();
    private readonly UpdateAppointmentStatusDTOValidator _updateValidator = new();

    [Fact]
    public void CreateValidator_ValidActiveStatus_Passes()
    {
        var dto = new CreateAppointmentStatusDTO { Status = AppointmentStatusEnum.Active };
        _createValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateValidator_ValidCompleteStatus_Passes()
    {
        var dto = new CreateAppointmentStatusDTO { Status = AppointmentStatusEnum.Complete };
        _createValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateValidator_NullStatus_Fails()
    {
        var dto = new CreateAppointmentStatusDTO { Status = null };
        _createValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void CreateValidator_InvalidStatusValue_Fails()
    {
        var dto = new CreateAppointmentStatusDTO { Status = (AppointmentStatusEnum)999 };
        _createValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void UpdateValidator_ValidCanceledStatus_Passes()
    {
        var dto = new UpdateAppointmentStatusDTO { Status = AppointmentStatusEnum.Canceled };
        _updateValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateValidator_ValidCompleteStatus_Passes()
    {
        var dto = new UpdateAppointmentStatusDTO { Status = AppointmentStatusEnum.Complete };
        _updateValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateValidator_NullStatus_Fails()
    {
        var dto = new UpdateAppointmentStatusDTO { Status = null };
        _updateValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void UpdateValidator_InvalidStatusValue_Fails()
    {
        var dto = new UpdateAppointmentStatusDTO { Status = (AppointmentStatusEnum)(-1) };
        _updateValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Status);
    }
}

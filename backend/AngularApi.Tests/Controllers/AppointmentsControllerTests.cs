using AngularApi.Filters;
using AngularApi.Models;
using AngularApi.Contracts.Services;
using System.Reflection;
using AngularApi.Controllers;
using AngularApi.Contracts.DTO;
using AngularApi.Contracts.Enums;
using AngularApi.Contracts.Models;
using AngularApi.Options;
using AngularApi.Services;
using AngularApi.Services.impelementation;
using AngularApi.Contracts.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;

namespace AngularApi.Tests.Controllers
{
    public class AppointmentsControllerTests : IDisposable
    {
        private readonly MedicalCenterDbContext _context;
        private readonly Mock<IAppointmentService> _appointmentServiceMock;
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly AppointmentsController _controller;

        public AppointmentsControllerTests()
        {
            var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new MedicalCenterDbContext(options);

            var store = new Mock<IUserStore<AppUser>>();
            _userManagerMock = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
            _appointmentServiceMock = new Mock<IAppointmentService>();

            _controller = CreateController(_appointmentServiceMock.Object);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "patient1"),
                new Claim(ClaimTypes.Role, "user"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _controller.ControllerContext.HttpContext.RequestServices = new Mock<IServiceProvider>().Object;
        }

        private AppointmentsController CreateController(IAppointmentService appointmentService) =>
            new(appointmentService, _userManagerMock.Object);

        [Fact]
        public void AppointmentMutatingActions_HaveValidateOwnershipAttributeForAppointment()
        {
            AssertValidateOwnership(typeof(AppointmentsController).GetMethod(nameof(AppointmentsController.GetAppointment))!);
            AssertValidateOwnership(typeof(AppointmentsController).GetMethod(nameof(AppointmentsController.UpdateAppointment))!);
            AssertValidateOwnership(typeof(AppointmentsController).GetMethod(nameof(AppointmentsController.DeleteAppointment))!);
        }

        private static void AssertValidateOwnership(MethodInfo method)
        {
            var attribute = method.GetCustomAttribute<ValidateOwnershipAttribute>();
            attribute.Should().NotBeNull();
            attribute!.ResourceType.Should().Be(ResourceType.Appointment);
            attribute.IdParameterName.Should().Be("id");
        }

        [Fact]
        public async Task GetAllAppointments_ReturnsAppointmentDtos()
        {
            var expected = new PagedResult<AppointmentDTO>
            {
                Items =
                [
                    new AppointmentDTO
                    {
                        AppointmentId = 1,
                        Doctor = new DoctorDTO { Specializations = ["Cardiology"] },
                        Patient = new PatientDTO { Name = "Patient1" }
                    }
                ],
                TotalCount = 1,
                CurrentPage = 1,
                PageSize = 10,
                PageCount = 1
            };
            _appointmentServiceMock
                .Setup(s => s.GetAllAppointmentsAsync(It.IsAny<PaginationParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetAllAppointments(new PaginationParameters());

            var paged = result.Value.Should().BeAssignableTo<PagedResult<AppointmentDTO>>().Subject;
            paged.Items.Should().HaveCount(1);
            paged.Items[0].Doctor.Specializations.Should().Contain("Cardiology");
            paged.Items[0].Patient.Name.Should().Be("Patient1");
        }

        [Fact]
        public async Task GetAppointment_NonExistingId_ReturnsNotFound()
        {
            _appointmentServiceMock
                .Setup(s => s.GetAppointmentByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            var result = await _controller.GetAppointment(1);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateAppointment_ValidInput_UpdatesAppointment()
        {
            var appointment = new Appointment { Id = 1, AppointmentTakenDate = DateTime.Now };
            var appointmentDto = new UpdateAppointmentDTO { Id = 1, AppointmentTakenDate = DateTime.Now.AddDays(1) };

            _appointmentServiceMock
                .Setup(s => s.GetAppointmentByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);
            _appointmentServiceMock
                .Setup(s => s.UpdateAppointmentAsync(1, appointmentDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.UpdateAppointment(1, appointmentDto);

            result.Should().BeOfType<NoContentResult>();
            _appointmentServiceMock.Verify(s => s.UpdateAppointmentAsync(1, appointmentDto, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAppointment_IdMismatch_ReturnsBadRequest()
        {
            var appointmentDto = new UpdateAppointmentDTO { Id = 2 };

            var result = await _controller.UpdateAppointment(1, appointmentDto);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Appointment ID mismatch.");
        }

        [Fact]
        public async Task PostAppointment_InvalidDoctorId_ReturnsBadRequest()
        {
            _userManagerMock.Setup(x => x.FindByIdAsync("patient1")).ReturnsAsync(new AppUser { Id = "patient1", UserName = "Patient1", Email = "patient1@example.com" });
            var appointment = new Appointment { Id = 1, DoctorId = "invalid-doctor" };
            _appointmentServiceMock
                .Setup(s => s.CreateAppointmentAsync(appointment, "patient1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((null, "Invalid DoctorId"));

            var result = await _controller.PostAppointment(appointment);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid DoctorId");
        }

        [Fact]
        public async Task PostAppointment_MissingDoctorId_ReturnsBadRequest()
        {
            _userManagerMock.Setup(x => x.FindByIdAsync("patient1")).ReturnsAsync(new AppUser { Id = "patient1", UserName = "Patient1", Email = "patient1@example.com" });
            var appointment = new Appointment { Id = 1 };
            _appointmentServiceMock
                .Setup(s => s.CreateAppointmentAsync(appointment, "patient1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((null, "DoctorId is required"));

            var result = await _controller.PostAppointment(appointment);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("DoctorId is required");
        }

        [Fact]
        public async Task PostAppointment_UserNotFound_ReturnsNotFound()
        {
            var appointment = new Appointment { Id = 1, DoctorId = "doctor-id" };
            _userManagerMock.Setup(x => x.FindByIdAsync("patient1")).ReturnsAsync((AppUser?)null);

            var result = await _controller.PostAppointment(appointment);

            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("User not found");
        }

        [Fact]
        public async Task PostAppointment_ValidInput_UsesConfigurationAndDoctorId()
        {
            var user = new AppUser { Id = "patient1", UserName = "Patient1", Email = "patient1@example.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync("patient1")).ReturnsAsync(user);

            var createdAppointment = new Appointment
            {
                Id = 1,
                DoctorId = "doctor-id",
                DoctorName = "Dr. Smith",
                MedicalCenterId = 9,
                Amount = 55,
                AppointmentStatusId = (int)AppointmentStatusEnum.Active,
                AppointmentTakenDate = DateTime.UtcNow
            };
            var appointment = new Appointment { DoctorId = "doctor-id", AppointmentTakenDate = DateTime.UtcNow };
            _appointmentServiceMock
                .Setup(s => s.CreateAppointmentAsync(appointment, "patient1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((createdAppointment, null));

            var result = await _controller.PostAppointment(appointment);

            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var returnedAppointment = createdResult.Value.Should().BeAssignableTo<Appointment>().Subject;
            returnedAppointment.DoctorId.Should().Be("doctor-id");
            returnedAppointment.DoctorName.Should().Be("Dr. Smith");
            returnedAppointment.MedicalCenterId.Should().Be(9);
            returnedAppointment.Amount.Should().Be(55);
            returnedAppointment.AppointmentStatusId.Should().Be((int)AppointmentStatusEnum.Active);
        }

        [Fact]
        public async Task PostAppointment_DelegatesToServiceAndReturnsCreated()
        {
            var user = new AppUser { Id = "patient1", UserName = "Patient1", Email = "patient1@example.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync("patient1")).ReturnsAsync(user);

            var createdAppointment = new Appointment
            {
                Id = 1,
                DoctorId = "doctor-id",
                DoctorName = "Dr. Smith",
                AppointmentTakenDate = DateTime.UtcNow
            };
            var appointment = new Appointment { DoctorId = "doctor-id", AppointmentTakenDate = DateTime.UtcNow };
            _appointmentServiceMock
                .Setup(s => s.CreateAppointmentAsync(appointment, "patient1", It.IsAny<CancellationToken>()))
                .ReturnsAsync((createdAppointment, null));

            var result = await _controller.PostAppointment(appointment);

            result.Result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task DeleteAppointment_ExistingId_DeletesAppointment()
        {
            _appointmentServiceMock
                .Setup(s => s.DeleteAppointmentAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.DeleteAppointment(1);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteAppointment_NonExistingId_ReturnsNotFound()
        {
            _appointmentServiceMock
                .Setup(s => s.DeleteAppointmentAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.DeleteAppointment(1);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetAppointmentsByPatient_ReturnsPatientAppointments()
        {
            var expected = new PagedResult<AppointmentDTO>
            {
                Items =
                [
                    new AppointmentDTO { AppointmentId = 1, Patient = new PatientDTO { PatientId = "patient1" } },
                    new AppointmentDTO { AppointmentId = 2, Patient = new PatientDTO { PatientId = "patient1" } }
                ],
                TotalCount = 2,
                CurrentPage = 1,
                PageSize = 10,
                PageCount = 1
            };
            _appointmentServiceMock
                .Setup(s => s.GetAppointmentsByPatientAsync("patient1", It.IsAny<PaginationParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetAppointmentsByPatient("patient1", new PaginationParameters());

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var paged = okResult.Value.Should().BeAssignableTo<PagedResult<AppointmentDTO>>().Subject;
            paged.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTodaysAppointments_ReturnsTodaysAppointments()
        {
            var expected = new PagedResult<AppointmentDTO>
            {
                Items = [new AppointmentDTO { AppointmentId = 1 }],
                TotalCount = 1,
                CurrentPage = 1,
                PageSize = 10,
                PageCount = 1
            };
            _appointmentServiceMock
                .Setup(s => s.GetTodaysAppointmentsAsync(It.IsAny<PaginationParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetTodaysAppointments(new PaginationParameters());

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var paged = okResult.Value.Should().BeAssignableTo<PagedResult<AppointmentDTO>>().Subject;
            paged.Items.Should().HaveCount(1);
            paged.Items[0].AppointmentId.Should().Be(1);
        }

        [Fact]
        public async Task GetTotalEarnings_ReturnsSumOfAmounts()
        {
            _appointmentServiceMock
                .Setup(s => s.GetTotalEarningsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(80m);

            var result = await _controller.GetPatientTotalEarnings();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAppointments_WithRealService_ReturnsPagedResults()
        {
            var appointmentService = new AppointmentService(
                _context,
                Microsoft.Extensions.Options.Options.Create(new AppointmentSettings { DefaultFee = 30, DefaultCenterId = 2 }),
                Mock.Of<IEmailService>(),
                new EmailTemplateService(Mock.Of<IWebHostEnvironment>(env =>
                    env.WebRootPath == Path.Combine(AppContext.BaseDirectory, "wwwroot"))),
                NullLogger<AppointmentService>.Instance);
            var controller = CreateController(appointmentService);
            controller.ControllerContext = _controller.ControllerContext;

            for (var i = 1; i <= 5; i++)
            {
                _context.Appointments.Add(new Appointment
                {
                    Id = i,
                    DoctorName = "Dr Smith",
                    PatientId = $"patient{i}",
                    AppointmentTakenDate = DateTime.UtcNow.AddDays(i)
                });
            }
            await _context.SaveChangesAsync();

            var result = await controller.GetAppointments(new PaginationParameters { Page = 1, PageSize = 3 });

            var paged = result.Value.Should().BeOfType<PagedResult<AppointmentDTO>>().Subject;
            paged.Items.Should().HaveCount(3);
            paged.TotalCount.Should().Be(5);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}

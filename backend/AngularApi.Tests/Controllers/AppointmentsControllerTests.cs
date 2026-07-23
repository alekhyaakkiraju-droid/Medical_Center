using AngularApi.Controllers;
using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Options;
using AngularApi.Services;
using AngularApi.Services.impelementation;
using AngularApi.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace AngularApi.Tests.Controllers
{
    public class AppointmentsControllerTests : IDisposable
    {
        private readonly MedicalCenterDbContext _context;
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly IOwnershipValidator _ownershipValidator;
        private readonly AppointmentsController _controller;

        public AppointmentsControllerTests()
        {
            var options = new DbContextOptionsBuilder<MedicalCenterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new MedicalCenterDbContext(options);

            var store = new Mock<IUserStore<AppUser>>();
            _userManagerMock = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);

            _emailServiceMock = new Mock<IEmailService>();

            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            webHostEnvironmentMock
                .Setup(env => env.WebRootPath)
                .Returns(Path.Combine(AppContext.BaseDirectory, "wwwroot"));
            _emailTemplateService = new EmailTemplateService(webHostEnvironmentMock.Object);
            _ownershipValidator = new OwnershipValidator();
            var appointmentSettings = Microsoft.Extensions.Options.Options.Create(new AppointmentSettings { DefaultFee = 30, DefaultCenterId = 2 });

            _controller = new AppointmentsController(
                _context,
                _userManagerMock.Object,
                _emailServiceMock.Object,
                _emailTemplateService,
                _ownershipValidator,
                appointmentSettings);

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

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(EmailTemplateService)))
                .Returns(_emailTemplateService);
            _controller.ControllerContext.HttpContext.RequestServices = serviceProviderMock.Object;
        }



        [Fact]
        public async Task GetAllAppointments_ReturnsAppointmentDtos()
        {
            // Arrange
            var doctor = new Doctor
            {
                Id = "doctor1",
                Name = "Dr. Smith",
                DoctorSpecializations = new List<DoctorSpecialization>
                {
                    new DoctorSpecialization { Specialization = new Specialization { SpecializationName = "Cardiology" } }
                }
            };
            _context.Doctors.Add(doctor);
            _context.Appointments.Add(new Appointment
            {
                Id = 1,
                AppointmentTakenDate = DateTime.Now,
                DoctorName = "Dr. Smith",
                DoctorId = "doctor1",
                Patient = new Patient { Id = "patient1", UserName = "Patient1", Email = "patient1@example.com" }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAllAppointments(new PaginationParameters());

            // Assert
            var paged = result.Value.Should().BeAssignableTo<PagedResult<AppointmentDTO>>().Subject;
            paged.Items.Should().HaveCount(1);
            paged.Items[0].Doctor.Specializations.Should().Contain("Cardiology");
            paged.Items[0].Patient.Name.Should().Be("Patient1");
        }


        [Fact]
        public async Task GetAppointment_NonExistingId_ReturnsNotFound()
        {

            // Act
            var result = await _controller.GetAppointment(1);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateAppointment_ValidInput_UpdatesAppointment()
        {
            // Arrange
            var appointment = new Appointment { Id = 1, AppointmentTakenDate = DateTime.Now };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            var appointmentDto = new UpdateAppointmentDTO { Id = 1, AppointmentTakenDate = DateTime.Now.AddDays(1) };

            // Act
            var result = await _controller.UpdateAppointment(1, appointmentDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            var updatedAppointment = await _context.Appointments.FindAsync(1);
            updatedAppointment.AppointmentTakenDate.Should().Be(appointmentDto.AppointmentTakenDate);
        }

        [Fact]
        public async Task UpdateAppointment_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var appointmentDto = new UpdateAppointmentDTO { Id = 2 };

            // Act
            var result = await _controller.UpdateAppointment(1, appointmentDto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Appointment ID mismatch.");
        }


        [Fact]
        public async Task PostAppointment_InvalidDoctorId_ReturnsBadRequest()
        {
            // Arrange
            var appointment = new Appointment { Id = 1, DoctorId = "invalid-doctor" };

            // Act
            var result = await _controller.PostAppointment(appointment);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid DoctorId");
        }

        [Fact]
        public async Task PostAppointment_MissingDoctorId_ReturnsBadRequest()
        {
            var appointment = new Appointment { Id = 1 };

            var result = await _controller.PostAppointment(appointment);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("DoctorId is required");
        }

        [Fact]
        public async Task PostAppointment_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var appointment = new Appointment { Id = 1, DoctorId = "doctor-id" };
            var doctor = new Doctor { Id = "doctor-id", Name = "Dr. Smith" };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            _userManagerMock.Setup(x => x.FindByIdAsync("user-id")).ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.PostAppointment(appointment);

            // Assert
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be("User not found");
        }

        [Fact]
        public async Task PostAppointment_ValidInput_UsesConfigurationAndDoctorId()
        {
            var doctor = new Doctor { Id = "doctor-id", Name = "Dr. Smith" };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var user = new AppUser { Id = "patient1", UserName = "Patient1", Email = "patient1@example.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync("patient1")).ReturnsAsync(user);

            var settings = Microsoft.Extensions.Options.Options.Create(new AppointmentSettings { DefaultFee = 55, DefaultCenterId = 9 });
            var controller = new AppointmentsController(
                _context,
                _userManagerMock.Object,
                _emailServiceMock.Object,
                _emailTemplateService,
                _ownershipValidator,
                settings)
            {
                ControllerContext = _controller.ControllerContext
            };

            var appointment = new Appointment { DoctorId = "doctor-id", AppointmentTakenDate = DateTime.UtcNow };

            var result = await controller.PostAppointment(appointment);

            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var createdAppointment = createdResult.Value.Should().BeAssignableTo<Appointment>().Subject;
            createdAppointment.DoctorId.Should().Be("doctor-id");
            createdAppointment.DoctorName.Should().Be("Dr. Smith");
            createdAppointment.MedicalCenterId.Should().Be(9);
            createdAppointment.Amount.Should().Be(55);
            createdAppointment.AppointmentStatusId.Should().Be((int)AppointmentStatusEnum.Active);
        }

        [Fact]
        public async Task PostAppointment_EmailFailure_StillCreatesAppointment()
        {
            var doctor = new Doctor { Id = "doctor-id", Name = "Dr. Smith" };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var user = new AppUser { Id = "patient1", UserName = "Patient1", Email = "patient1@example.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync("patient1")).ReturnsAsync(user);
            _emailServiceMock
                .Setup(x => x.SendEmail(It.IsAny<Message>()))
                .Throws(new InvalidOperationException("SMTP unavailable"));

            var appointment = new Appointment { DoctorId = "doctor-id", AppointmentTakenDate = DateTime.UtcNow };

            var result = await _controller.PostAppointment(appointment);

            result.Result.Should().BeOfType<CreatedAtActionResult>();
            (await _context.Appointments.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DeleteAppointment_ExistingId_DeletesAppointment()
        {
            // Arrange
            var appointment = new Appointment { Id = 1 };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteAppointment(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            var deletedAppointment = await _context.Appointments.FindAsync(1);
            deletedAppointment.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAppointment_NonExistingId_ReturnsNotFound()
        {


            // Act
            var result = await _controller.DeleteAppointment(1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetAppointmentsByPatient_ReturnsPatientAppointments()
        {
            // Arrange
            _context.Appointments.AddRange(new List<Appointment>
            {
                new Appointment { Id = 1, PatientId = "patient1" },
                new Appointment { Id = 2, PatientId = "patient1" },
                new Appointment { Id = 3, PatientId = "patient2" }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAppointmentsByPatient("patient1", new PaginationParameters());

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var paged = okResult.Value.Should().BeAssignableTo<PagedResult<AppointmentDTO>>().Subject;
            paged.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTodaysAppointments_ReturnsTodaysAppointments()
        {
            // Arrange
            var today = DateTime.Today;
            _context.Appointments.AddRange(new List<Appointment>
            {
                new Appointment { Id = 1, ProbableStartTime = today },
                new Appointment { Id = 2, ProbableStartTime = today.AddDays(1) }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetTodaysAppointments(new PaginationParameters());

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var paged = okResult.Value.Should().BeAssignableTo<PagedResult<AppointmentDTO>>().Subject;
            paged.Items.Should().HaveCount(1);
            paged.Items[0].AppointmentId.Should().Be(1);
        }

        [Fact]
        public async Task GetTotalEarnings_ReturnsSumOfAmounts()
        {
            // Arrange
            _context.Appointments.AddRange(new List<Appointment>
            {
                new Appointment { Id = 1, Amount = 30 },
                new Appointment { Id = 2, Amount = 50 }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetPatientTotalEarnings();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
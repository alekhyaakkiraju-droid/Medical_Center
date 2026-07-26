using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AngularApi.DTO;
using AngularApi.Models;
using AngularApi.Services;
using AngularApi.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AngularApi.Tests.Contracts;

public abstract class ContractTestBase : IClassFixture<MedicalCenterWebApplicationFactory>
{
    protected const string ContractPatientEmail = "contract-patient@example.com";
    protected const string ContractDoctorEmail = "contract-doctor@example.com";
    protected const string ContractAdminEmail = "contract-admin@example.com";
    protected const string ContractPassword = "ContractTest123!";
    protected const string ContractDoctorId = "contract-doctor-id";
    protected readonly MedicalCenterWebApplicationFactory Factory;
    protected ContractTestBase(MedicalCenterWebApplicationFactory factory) => Factory = factory;
    protected static void AssertPagedResultShape(JsonDocument document)
    {
        var root = document.RootElement;
        root.TryGetProperty("items", out _).Should().BeTrue();
        root.TryGetProperty("totalCount", out var totalCount).Should().BeTrue();
        totalCount.ValueKind.Should().Be(JsonValueKind.Number);
        root.TryGetProperty("pageCount", out _).Should().BeTrue();
        root.TryGetProperty("currentPage", out _).Should().BeTrue();
        root.TryGetProperty("pageSize", out _).Should().BeTrue();
    }
    protected static JsonDocument ParseJson(string json) => JsonDocument.Parse(json);
    protected HttpClient CreateClient(string? ipAddress = "203.0.113.60")
    {
        var client = AntiforgeryTestHelper.CreateClient(Factory);
        if (!string.IsNullOrEmpty(ipAddress)) client.DefaultRequestHeaders.Add("X-Test-Client-Ip", ipAddress);
        return client;
    }
    protected async Task<HttpClient> LoginAsync(string email, string password)
    {
        var client = CreateClient();
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);
        var response = await client.PostAsJsonAsync("/api/Account/login", new LogInUserDTO { Email = email, Password = password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AntiforgeryTestHelper.ImportAuthCookies(response, client);
        await AntiforgeryTestHelper.ApplyAntiforgeryTokenAsync(client);
        return client;
    }
    protected async Task EnsureRolesAsync()
    {
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>().EnsureRolesCreatedAsync();
    }
    protected async Task<string> SeedPatientUserAsync(string email = ContractPatientEmail)
    {
        await EnsureRolesAsync();
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null) return existing.Id;
        var patient = new Patient { UserName = email, Email = email, EmailConfirmed = true };
        (await userManager.CreateAsync(patient, ContractPassword)).Succeeded.Should().BeTrue();
        await userManager.AddToRoleAsync(patient, "user");
        return patient.Id;
    }
    protected async Task<string> SeedDoctorUserAsync(string email = ContractDoctorEmail, string doctorId = ContractDoctorId)
    {
        await EnsureRolesAsync();
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null) return existing.Id;
        var doctor = new Doctor { Id = doctorId, UserName = email, Email = email, EmailConfirmed = true, Name = "Contract Doctor" };
        (await userManager.CreateAsync(doctor, ContractPassword)).Succeeded.Should().BeTrue();
        await userManager.AddToRoleAsync(doctor, "doctor");
        if (!context.Doctors.Any(d => d.Id == doctorId)) { context.Doctors.Add(new Doctor { Id = doctorId, Name = "Contract Doctor" }); await context.SaveChangesAsync(); }
        return doctor.Id;
    }
    protected async Task<string> SeedAdminUserAsync(string email = ContractAdminEmail)
    {
        await EnsureRolesAsync();
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null) return existing.Id;
        var admin = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
        (await userManager.CreateAsync(admin, ContractPassword)).Succeeded.Should().BeTrue();
        await userManager.AddToRoleAsync(admin, "admin");
        return admin.Id;
    }
    protected async Task SeedDoctorForAppointmentsAsync() => await SeedDoctorUserAsync();
}

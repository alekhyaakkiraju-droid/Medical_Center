using AngularApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AngularApi.Tests.Services;

public class EnsureRolesCreatedAsyncTests
{
    [Fact]
    public async Task EnsureRolesCreatedAsync_CreatesMissingRoles()
    {
        var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(),
            null, null, null, null);

        roleManagerMock
            .Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        roleManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        await roleManagerMock.Object.EnsureRolesCreatedAsync();

        roleManagerMock.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "admin")), Times.Once);
        roleManagerMock.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "user")), Times.Once);
        roleManagerMock.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "doctor")), Times.Once);
    }

    [Fact]
    public async Task EnsureRolesCreatedAsync_SkipsExistingRoles()
    {
        var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(),
            null, null, null, null);

        roleManagerMock
            .Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        await roleManagerMock.Object.EnsureRolesCreatedAsync();

        roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
    }
}

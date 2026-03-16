using Cheetah.Core.Identity.DataAccess.Exceptions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Crm.Identity.Application.Tests.Commands;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<RoleManager<CrmIdentityRole>> _roleManagerMock;
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        var roleStoreMock = new Mock<IRoleStore<CrmIdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<CrmIdentityRole>>(
            roleStoreMock.Object, null, null, null, null);
        _handler = new CreateRoleCommandHandler(_roleManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidName_ShouldCreateRoleAndReturnId()
    {
        // Arrange
        var command = new CreateRoleCommand("admin");

        _roleManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<CrmIdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        _roleManagerMock.Verify(m => m.CreateAsync(It.IsAny<CrmIdentityRole>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var command = new CreateRoleCommand(name!);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task HandleAsync_WhenCreateFails_ShouldThrowIdentityException()
    {
        // Arrange
        var command = new CreateRoleCommand("admin");
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "DuplicateRoleName", Description = "Role already exists" });

        _roleManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<CrmIdentityRole>()))
            .ReturnsAsync(failedResult);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<IdentityException>(act);
    }
}

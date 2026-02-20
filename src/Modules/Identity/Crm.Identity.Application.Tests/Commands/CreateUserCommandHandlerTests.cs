using Cheetah.Core.Identity.DataAccess.Exceptions;
using Crm.Identity.Application.Commands;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Crm.Identity.Application.Tests.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<UserManager<CrmUser>> _userManagerMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<CrmUser>>();
        _userManagerMock = new Mock<UserManager<CrmUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        _handler = new CreateUserCommandHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateUserAndReturnId()
    {
        // Arrange
        var command = new CreateUserCommand("johndoe", "john@example.com", "P@ssw0rd!");

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<CrmUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<CrmUser>(), command.Password), Times.Once);
    }

    [Theory]
    [InlineData(null, "john@example.com")]
    [InlineData("", "john@example.com")]
    [InlineData("   ", "john@example.com")]
    [InlineData("johndoe", null)]
    [InlineData("johndoe", "")]
    [InlineData("johndoe", "   ")]
    public async Task HandleAsync_WithInvalidUserNameOrEmail_ShouldThrowArgumentException(
        string? userName, string? email)
    {
        // Arrange
        var command = new CreateUserCommand(userName!, email!, "P@ssw0rd!");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task HandleAsync_WhenCreateFails_ShouldThrowIdentityException()
    {
        // Arrange
        var command = new CreateUserCommand("johndoe", "john@example.com", "P@ssw0rd!");
        var failedResult = IdentityResult.Failed(new IdentityError { Code = "PasswordTooWeak", Description = "Password is too weak" });

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<CrmUser>(), It.IsAny<string>()))
            .ReturnsAsync(failedResult);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<IdentityException>(act);
    }
}

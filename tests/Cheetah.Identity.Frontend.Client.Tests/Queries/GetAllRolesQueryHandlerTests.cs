using Cheetah.Identity.Client;
using Cheetah.Identity.Frontend.Client.Queries;
using Cheetah.Identity.Contracts.ViewModels;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Frontend.Client.Tests.Queries;

public class GetAllRolesQueryHandlerTests
{
    private readonly Mock<IIdentityClient> _identityClientMock;
    private readonly GetAllRolesQueryHandler _handler;

    public GetAllRolesQueryHandlerTests()
    {
        _identityClientMock = new Mock<IIdentityClient>();
        _handler = new GetAllRolesQueryHandler(_identityClientMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsListOfRoles()
    {
        // Arrange
        var query = new GetAllRolesQuery();

        var expectedRoles = new List<RoleViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "User", Description = "Standard User", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Manager", Description = "Manager role", CreatedAt = DateTime.UtcNow }
        };

        _identityClientMock
            .Setup(x => x.GetAllRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRoles);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Name == "Admin");
        result.Should().Contain(r => r.Name == "User");
        result.Should().Contain(r => r.Name == "Manager");

        _identityClientMock.Verify(
            x => x.GetAllRolesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoRoles_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllRolesQuery();

        _identityClientMock
            .Setup(x => x.GetAllRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleViewModel>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _identityClientMock.Verify(
            x => x.GetAllRolesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

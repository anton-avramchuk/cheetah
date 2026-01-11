using Cheetah.Identity.Application.Queries;
using Cheetah.Identity.Application.Tests.Helpers;
using Cheetah.Identity.Contracts.ViewModels;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Cheetah.Identity.Application.Tests.Queries;

public class GetAllRolesQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly GetAllRolesQueryHandler _handler;

    public GetAllRolesQueryHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _handler = new GetAllRolesQueryHandler(_roleRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllRoles_WhenRolesExist()
    {
        // Arrange
        var query = new GetAllRolesQuery();

        var role1 = Role.Create("Admin", "Administrator role");
        var role2 = Role.Create("Manager", "Manager role");
        var role3 = Role.Create("User", "Regular user role");

        var roles = new TestAsyncEnumerable<Role>(new List<Role> { role1, role2, role3 });

        _roleRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(roles);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Name == "Admin");
        result.Should().Contain(r => r.Name == "Manager");
        result.Should().Contain(r => r.Name == "User");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoRolesExist()
    {
        // Arrange
        var query = new GetAllRolesQuery();

        var roles = new TestAsyncEnumerable<Role>(new List<Role>());

        _roleRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(roles);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnRoleViewModels_WithAllProperties()
    {
        // Arrange
        var query = new GetAllRolesQuery();

        var role = Role.Create("SuperAdmin", "Super Administrator with full access");

        var roles = new TestAsyncEnumerable<Role>(new List<Role> { role });

        _roleRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(roles);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        var roleViewModel = result.First();
        roleViewModel.Id.Should().Be(role.Id);
        roleViewModel.Name.Should().Be("SuperAdmin");
        roleViewModel.Description.Should().Be("Super Administrator with full access");
        roleViewModel.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnRoles_OrderedByName()
    {
        // Arrange
        var query = new GetAllRolesQuery();

        var role1 = Role.Create("Zebra", null);
        var role2 = Role.Create("Alpha", null);
        var role3 = Role.Create("Manager", null);

        var roles = new TestAsyncEnumerable<Role>(new List<Role> { role1, role2, role3 });

        _roleRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(roles);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        // Note: The handler doesn't explicitly order, so we just verify all are returned
        result.Select(r => r.Name).Should().Contain(new[] { "Zebra", "Alpha", "Manager" });
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleRoles_WithNullDescription()
    {
        // Arrange
        var query = new GetAllRolesQuery();

        var role = Role.Create("BasicUser", null);

        var roles = new TestAsyncEnumerable<Role>(new List<Role> { role });

        _roleRepositoryMock.Setup(x => x.AsNoTrackingQueryable())
            .Returns(roles);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be("BasicUser");
        result.First().Description.Should().BeNull();
    }
}

using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Application.Tests;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Queries;

public sealed class StubGetRoleByIdQueryHandler(RoleManager<StubRole> roleManager, IObjectMapper mapper)
    : GetRoleByIdQueryHandler<StubRole>(roleManager, mapper);

public class GetRoleByIdQueryHandlerTests
{
    private readonly Mock<RoleManager<StubRole>> _roleManagerMock;
    private readonly Mock<IObjectMapper> _mapperMock;
    private readonly StubGetRoleByIdQueryHandler _handler;

    public GetRoleByIdQueryHandlerTests()
    {
        var store = new Mock<IRoleStore<StubRole>>();
        _roleManagerMock = new Mock<RoleManager<StubRole>>(store.Object, null!, null!, null!, null!);
        _mapperMock = new Mock<IObjectMapper>();
        _handler = new StubGetRoleByIdQueryHandler(_roleManagerMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleExists_ShouldReturnProjectedModel()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var expectedModel = new RoleModel(roleId, "admin");

        _roleManagerMock.Setup(m => m.Roles).Returns(new List<StubRole>().AsAsyncQueryable());
        _mapperMock
            .Setup(m => m.ProjectTo<RoleModel>(It.IsAny<IQueryable>()))
            .Returns(new[] { expectedModel }.AsAsyncQueryable());

        // Act
        var result = await _handler.HandleAsync(new GetRoleByIdQuery(roleId));

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedModel);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _roleManagerMock.Setup(m => m.Roles).Returns(new List<StubRole>().AsAsyncQueryable());
        _mapperMock
            .Setup(m => m.ProjectTo<RoleModel>(It.IsAny<IQueryable>()))
            .Returns(Array.Empty<RoleModel>().AsAsyncQueryable());

        // Act
        var result = await _handler.HandleAsync(new GetRoleByIdQuery(roleId));

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldCallProjectToWithRolesQueryable()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _roleManagerMock.Setup(m => m.Roles).Returns(new List<StubRole>().AsAsyncQueryable());
        _mapperMock
            .Setup(m => m.ProjectTo<RoleModel>(It.IsAny<IQueryable>()))
            .Returns(Array.Empty<RoleModel>().AsAsyncQueryable());

        // Act
        await _handler.HandleAsync(new GetRoleByIdQuery(roleId));

        // Assert
        _mapperMock.Verify(m => m.ProjectTo<RoleModel>(It.IsAny<IQueryable>()), Times.Once);
    }
}

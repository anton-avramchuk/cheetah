using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Application.Tests;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<UserManager<StubUser>> _userManagerMock;
    private readonly Mock<IObjectMapper> _mapperMock;
    private readonly GetUserByIdQueryHandler<StubUser, StubRole, UserDetailModel> _handler;

    public GetUserByIdQueryHandlerTests()
    {
        var store = new Mock<IUserStore<StubUser>>();
        _userManagerMock = new Mock<UserManager<StubUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _mapperMock = new Mock<IObjectMapper>();
        _handler = new GetUserByIdQueryHandler<StubUser, StubRole, UserDetailModel>(_userManagerMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_ShouldReturnProjectedModel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedModel = new UserDetailModel(userId, "johndoe", "john@example.com", false, []);

        _userManagerMock.Setup(m => m.Users).Returns(new List<StubUser>().AsAsyncQueryable());
        _mapperMock
            .Setup(m => m.ProjectTo<UserDetailModel>(It.IsAny<IQueryable>()))
            .Returns(new[] { expectedModel }.AsAsyncQueryable());

        // Act
        var result = await _handler.HandleAsync(new GetUserByIdQuery<UserDetailModel>(userId));

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedModel);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userManagerMock.Setup(m => m.Users).Returns(new List<StubUser>().AsAsyncQueryable());
        _mapperMock
            .Setup(m => m.ProjectTo<UserDetailModel>(It.IsAny<IQueryable>()))
            .Returns(Array.Empty<UserDetailModel>().AsAsyncQueryable());

        // Act
        var result = await _handler.HandleAsync(new GetUserByIdQuery<UserDetailModel>(userId));

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldCallProjectToWithUsersQueryable()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userManagerMock.Setup(m => m.Users).Returns(new List<StubUser>().AsAsyncQueryable());
        _mapperMock
            .Setup(m => m.ProjectTo<UserDetailModel>(It.IsAny<IQueryable>()))
            .Returns(Array.Empty<UserDetailModel>().AsAsyncQueryable());

        // Act
        await _handler.HandleAsync(new GetUserByIdQuery<UserDetailModel>(userId));

        // Assert
        _mapperMock.Verify(m => m.ProjectTo<UserDetailModel>(It.IsAny<IQueryable>()), Times.Once);
    }
}

using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Application.Tests;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Queries;

public class GetUsersGridQueryHandlerTests
{
    private readonly Mock<IGridRepository<StubUser>> _repositoryMock;
    private readonly GetUsersGridQueryHandler<StubUser, StubRole, UserModel> _handler;

    public GetUsersGridQueryHandlerTests()
    {
        _repositoryMock = new Mock<IGridRepository<StubUser>>();
        _handler = new GetUsersGridQueryHandler<StubUser, StubRole, UserModel>(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnGridResultFromRepository()
    {
        // Arrange
        var users = new[] { new UserModel(Guid.NewGuid(), "johndoe", "john@example.com", true) };
        var expectedResult = new GridResult<UserModel>(users, 1);
        var query = new GetUsersGridQuery<UserModel>(1, 10, [], null);

        _repositoryMock
            .Setup(r => r.GetGridAsync<UserModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassPaginationToRepository()
    {
        // Arrange
        var query = new GetUsersGridQuery<UserModel>(2, 50, [], null);

        _repositoryMock
            .Setup(r => r.GetGridAsync<UserModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridResult<UserModel>([], 0));

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _repositoryMock.Verify(r => r.GetGridAsync<UserModel>(
            It.Is<GridRequest>(req => req.Page == 2 && req.PageSize == 50),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassSortAndFilterToRepository()
    {
        // Arrange
        var sort = new List<SortDescriptor> { new() { Field = "UserName", Dir = "desc" } };
        var filter = new FilterDescriptor { Field = "Email", Value = "example.com" };
        var query = new GetUsersGridQuery<UserModel>(1, 10, sort, filter);

        _repositoryMock
            .Setup(r => r.GetGridAsync<UserModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridResult<UserModel>([], 0));

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _repositoryMock.Verify(r => r.GetGridAsync<UserModel>(
            It.Is<GridRequest>(req =>
                req.Sort == sort &&
                req.Filter == filter),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

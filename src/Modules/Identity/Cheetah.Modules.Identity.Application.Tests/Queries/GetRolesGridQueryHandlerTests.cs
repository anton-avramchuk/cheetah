using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Application.Tests;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Queries;

public class GetRolesGridQueryHandlerTests
{
    private readonly Mock<IGridRepository<StubRole>> _repositoryMock;
    private readonly GetRolesGridQueryHandler<StubRole, RoleModel> _handler;

    public GetRolesGridQueryHandlerTests()
    {
        _repositoryMock = new Mock<IGridRepository<StubRole>>();
        _handler = new GetRolesGridQueryHandler<StubRole, RoleModel>(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnGridResultFromRepository()
    {
        // Arrange
        var roles = new[] { new RoleModel(Guid.NewGuid(), "admin") };
        var expectedResult = new GridResult<RoleModel>(roles, 1);
        var query = new GetRolesGridQuery<RoleModel>(1, 10, [], null);

        _repositoryMock
            .Setup(r => r.GetGridAsync<RoleModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
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
        var query = new GetRolesGridQuery<RoleModel>(3, 25, [], null);

        _repositoryMock
            .Setup(r => r.GetGridAsync<RoleModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridResult<RoleModel>([], 0));

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _repositoryMock.Verify(r => r.GetGridAsync<RoleModel>(
            It.Is<GridRequest>(req => req.Page == 3 && req.PageSize == 25),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassSortAndFilterToRepository()
    {
        // Arrange
        var sort = new List<SortDescriptor> { new() { Field = "Name", Dir = "asc" } };
        var filter = new FilterDescriptor { Field = "Name", Value = "admin" };
        var query = new GetRolesGridQuery<RoleModel>(1, 10, sort, filter);

        _repositoryMock
            .Setup(r => r.GetGridAsync<RoleModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridResult<RoleModel>([], 0));

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _repositoryMock.Verify(r => r.GetGridAsync<RoleModel>(
            It.Is<GridRequest>(req =>
                req.Sort == sort &&
                req.Filter == filter),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

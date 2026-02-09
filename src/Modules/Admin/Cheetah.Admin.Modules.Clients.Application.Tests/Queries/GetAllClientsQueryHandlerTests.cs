using Cheetah.Admin.Modules.Clients.Application.Queries;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.Grid;
using Shouldly;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Queries;

public class GetAllClientsQueryHandlerTests
{
    private readonly Mock<IGridRepository<Client>> _repositoryMock;
    private readonly GetAllClientsQueryHandler _handler;

    public GetAllClientsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IGridRepository<Client>>();
        _handler = new GetAllClientsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithClients_ShouldReturnClientModels()
    {
        // Arrange
        var clientModels = new List<ClientModel>
        {
            new(Guid.NewGuid(), "Client 1", "Description 1", null),
            new(Guid.NewGuid(), "Client 2", "Description 2", null),
            new(Guid.NewGuid(), "Client 3", null, null)
        };

        var gridResult = new GridResult<ClientModel>(clientModels, 3);

        _repositoryMock
            .Setup(r => r.GetGridAsync<ClientModel>(
                It.IsAny<GridRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(gridResult);

        var query = new GetAllClientsQuery(1, 10, new List<SortDescriptor>(), null);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        var dataList = result.Data.ToList();
        dataList.Count.ShouldBe(3);
        result.Total.ShouldBe(3);
        dataList[0].Name.ShouldBe("Client 1");
        dataList[1].Name.ShouldBe("Client 2");
        dataList[2].Name.ShouldBe("Client 3");
    }

    [Fact]
    public async Task HandleAsync_WithNoClients_ShouldReturnEmptyResult()
    {
        // Arrange
        var gridResult = new GridResult<ClientModel>(new List<ClientModel>(), 0);

        _repositoryMock
            .Setup(r => r.GetGridAsync<ClientModel>(
                It.IsAny<GridRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(gridResult);

        var query = new GetAllClientsQuery(1, 10, new List<SortDescriptor>(), null);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Data.ShouldBeEmpty();
        result.Total.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectGridRequestToRepository()
    {
        // Arrange
        var gridResult = new GridResult<ClientModel>(new List<ClientModel>(), 0);
        GridRequest? capturedRequest = null;

        _repositoryMock
            .Setup(r => r.GetGridAsync<ClientModel>(
                It.IsAny<GridRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<GridRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(gridResult);

        var sort = new List<SortDescriptor> { new() { Field = "Name", Dir = "asc" } };
        var filter = new FilterDescriptor { Field = "Name", Operator = "contains", Value = "test" };
        var query = new GetAllClientsQuery(2, 25, sort, filter);

        // Act
        await _handler.HandleAsync(query);

        // Assert
        capturedRequest.ShouldNotBeNull();
        capturedRequest!.Page.ShouldBe(2);
        capturedRequest.PageSize.ShouldBe(25);
        capturedRequest.Sort.ShouldBe(sort);
        capturedRequest.Filter.ShouldBe(filter);
    }
}

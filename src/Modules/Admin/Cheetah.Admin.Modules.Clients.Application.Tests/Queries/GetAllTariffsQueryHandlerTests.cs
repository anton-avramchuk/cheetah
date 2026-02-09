using Cheetah.Admin.Modules.Clients.Application.Queries;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.Grid;
using Shouldly;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Queries;

public class GetAllTariffsQueryHandlerTests
{
    private readonly Mock<IGridRepository<Tariff>> _repositoryMock;
    private readonly GetAllTariffsQueryHandler _handler;

    public GetAllTariffsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IGridRepository<Tariff>>();
        _handler = new GetAllTariffsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithTariffs_ShouldReturnTariffModels()
    {
        // Arrange
        var tariffModels = new List<TariffModel>
        {
            new(Guid.NewGuid(), "Basic", "Basic plan", 9.99m, "USD", true),
            new(Guid.NewGuid(), "Premium", "Premium plan", 29.99m, "EUR", false)
        };

        var gridResult = new GridResult<TariffModel>(tariffModels, 2);

        _repositoryMock
            .Setup(r => r.GetGridAsync<TariffModel>(
                It.IsAny<GridRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(gridResult);

        var query = new GetAllTariffsQuery(1, 10, new List<SortDescriptor>(), null);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        var dataList = result.Data.ToList();
        dataList.Count.ShouldBe(2);
        result.Total.ShouldBe(2);
        dataList[0].Name.ShouldBe("Basic");
        dataList[0].Price.ShouldBe(9.99m);
        dataList[0].Currency.ShouldBe("USD");
        dataList[0].IsActive.ShouldBeTrue();
        dataList[1].Name.ShouldBe("Premium");
        dataList[1].IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithNoTariffs_ShouldReturnEmptyResult()
    {
        // Arrange
        var gridResult = new GridResult<TariffModel>(new List<TariffModel>(), 0);

        _repositoryMock
            .Setup(r => r.GetGridAsync<TariffModel>(
                It.IsAny<GridRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(gridResult);

        var query = new GetAllTariffsQuery(1, 10, new List<SortDescriptor>(), null);

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
        var gridResult = new GridResult<TariffModel>(new List<TariffModel>(), 0);
        GridRequest? capturedRequest = null;

        _repositoryMock
            .Setup(r => r.GetGridAsync<TariffModel>(
                It.IsAny<GridRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<GridRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(gridResult);

        var sort = new List<SortDescriptor> { new() { Field = "Name", Dir = "asc" } };
        var filter = new FilterDescriptor { Field = "Price", Operator = "gte", Value = "10" };
        var query = new GetAllTariffsQuery(3, 50, sort, filter);

        // Act
        await _handler.HandleAsync(query);

        // Assert
        capturedRequest.ShouldNotBeNull();
        capturedRequest!.Page.ShouldBe(3);
        capturedRequest.PageSize.ShouldBe(50);
        capturedRequest.Sort.ShouldBe(sort);
        capturedRequest.Filter.ShouldBe(filter);
    }
}

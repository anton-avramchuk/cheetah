using Cheetah.Admin.Modules.Clients.Application.Queries;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.Grid;
using Shouldly;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Queries;

public class GetAllTariffsQueryHandlerTests
{
    private readonly Mock<ITariffRepository> _repositoryMock;
    private readonly Mock<IGridQueryService> _gridServiceMock;
    private readonly GetAllTariffsQueryHandler _handler;

    public GetAllTariffsQueryHandlerTests()
    {
        _repositoryMock = new Mock<ITariffRepository>();
        _gridServiceMock = new Mock<IGridQueryService>();
        _handler = new GetAllTariffsQueryHandler(_repositoryMock.Object, _gridServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithTariffs_ShouldReturnTariffModels()
    {
        // Arrange
        // TariffModel(Guid Id, string Name, string? Description, decimal Price, string Currency, bool IsActive)
        var tariffModels = new List<TariffModel>
        {
            new(Guid.NewGuid(), "Basic", "Basic plan", 9.99m, "USD", true),
            new(Guid.NewGuid(), "Premium", "Premium plan", 29.99m, "EUR", false)
        };

        var gridResult = new GridResult<TariffModel>(tariffModels, 2);

        _repositoryMock
            .Setup(r => r.AsNoTrackingQueryable())
            .Returns(new List<Tariff>().AsQueryable());

        _gridServiceMock
            .Setup(g => g.ExecuteAsync<Tariff, TariffModel>(
                It.IsAny<IQueryable<Tariff>>(),
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
            .Setup(r => r.AsNoTrackingQueryable())
            .Returns(new List<Tariff>().AsQueryable());

        _gridServiceMock
            .Setup(g => g.ExecuteAsync<Tariff, TariffModel>(
                It.IsAny<IQueryable<Tariff>>(),
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
    public async Task HandleAsync_ShouldPassCorrectGridRequestToService()
    {
        // Arrange
        var gridResult = new GridResult<TariffModel>(new List<TariffModel>(), 0);
        GridRequest? capturedRequest = null;

        _repositoryMock
            .Setup(r => r.AsNoTrackingQueryable())
            .Returns(new List<Tariff>().AsQueryable());

        _gridServiceMock
            .Setup(g => g.ExecuteAsync<Tariff, TariffModel>(
                It.IsAny<IQueryable<Tariff>>(),
                It.IsAny<GridRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<IQueryable<Tariff>, GridRequest, CancellationToken>((_, req, _) => capturedRequest = req)
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

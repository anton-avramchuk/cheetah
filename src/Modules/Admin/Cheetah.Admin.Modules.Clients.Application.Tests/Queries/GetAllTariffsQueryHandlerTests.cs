using Cheetah.Admin.Modules.Clients.Application.Queries;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Shouldly;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Queries;

public class GetAllTariffsQueryHandlerTests
{
    private readonly Mock<ITariffRepository> _repositoryMock;
    private readonly GetAllTariffsQueryHandler _handler;

    public GetAllTariffsQueryHandlerTests()
    {
        _repositoryMock = new Mock<ITariffRepository>();
        _handler = new GetAllTariffsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithTariffs_ShouldReturnTariffModels()
    {
        // Arrange
        var tariffs = new List<Tariff>
        {
            Tariff.Create("Basic", 9.99m, "USD", "Basic plan", true),
            Tariff.Create("Premium", 29.99m, "EUR", "Premium plan", false)
        };

        _repositoryMock
            .Setup(r => r.GetAllNoTrackingAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tariffs);

        var query = new GetAllTariffsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("Basic");
        result[0].Price.ShouldBe(9.99m);
        result[0].Currency.ShouldBe("USD");
        result[0].IsActive.ShouldBeTrue();
        result[1].Name.ShouldBe("Premium");
        result[1].IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithNoTariffs_ShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllNoTrackingAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tariff>());

        var query = new GetAllTariffsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.ShouldBeEmpty();
    }
}

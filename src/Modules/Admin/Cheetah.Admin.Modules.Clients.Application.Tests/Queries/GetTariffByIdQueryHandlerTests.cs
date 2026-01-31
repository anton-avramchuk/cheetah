using Cheetah.Admin.Modules.Clients.Application.Queries;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Queries;

public class GetTariffByIdQueryHandlerTests
{
    private readonly Mock<ITariffRepository> _repositoryMock;
    private readonly GetTariffByIdQueryHandler _handler;

    public GetTariffByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<ITariffRepository>();
        _handler = new GetTariffByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingTariff_ShouldReturnTariffModel()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var tariff = Tariff.Create("Basic", 9.99m, "USD", "Basic plan", true);
        typeof(Tariff).GetProperty("Id")!.SetValue(tariff, tariffId);

        _repositoryMock
            .Setup(r => r.GetByIdNoTrackingAsync(tariffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tariff);

        var query = new GetTariffByIdQuery(tariffId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(tariffId);
        result.Name.Should().Be("Basic");
        result.Price.Should().Be(9.99m);
        result.Currency.Should().Be("USD");
        result.Description.Should().Be("Basic plan");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingTariff_ShouldReturnNull()
    {
        // Arrange
        var tariffId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdNoTrackingAsync(tariffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tariff?)null);

        var query = new GetTariffByIdQuery(tariffId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeNull();
    }
}

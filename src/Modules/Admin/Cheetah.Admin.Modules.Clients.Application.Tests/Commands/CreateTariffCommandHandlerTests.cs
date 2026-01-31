using Cheetah.Admin.Modules.Clients.Application.Commands;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.Events;
using FluentAssertions;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Commands;

public class CreateTariffCommandHandlerTests
{
    private readonly Mock<ITariffRepository> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateTariffCommandHandler _handler;

    public CreateTariffCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITariffRepository>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateTariffCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateTariffAndReturnId()
    {
        // Arrange
        var command = new CreateTariffCommand("Basic Plan", "Description", 9.99m, "USD", true);
        Tariff? capturedTariff = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Tariff>()))
            .Callback<Tariff>(t => capturedTariff = t);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeEmpty();
        capturedTariff.Should().NotBeNull();
        capturedTariff!.Name.Should().Be("Basic Plan");
        capturedTariff.Description.Should().Be("Description");
        capturedTariff.Price.Should().Be(9.99m);
        capturedTariff.Currency.Should().Be("USD");
        capturedTariff.IsActive.Should().BeTrue();

        _repositoryMock.Verify(r => r.Add(It.IsAny<Tariff>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveTariff_ShouldCreateInactiveTariff()
    {
        // Arrange
        var command = new CreateTariffCommand("Plan", null, 10m, "EUR", false);
        Tariff? capturedTariff = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Tariff>()))
            .Callback<Tariff>(t => capturedTariff = t);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        capturedTariff!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateTariffCommand("", "Description", 10m, "USD", true);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCurrency_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateTariffCommand("Plan", null, 10m, "US", true);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_WithNegativePrice_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var command = new CreateTariffCommand("Plan", null, -10m, "USD", true);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}

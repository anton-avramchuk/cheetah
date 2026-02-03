using Cheetah.Admin.Modules.Clients.Application.Commands;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Shouldly;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Commands;

public class UpdateTariffCommandHandlerTests
{
    private readonly Mock<ITariffRepository> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly UpdateTariffCommandHandler _handler;

    public UpdateTariffCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITariffRepository>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new UpdateTariffCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldUpdateTariff()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var existingTariff = Tariff.Create("Original", 10m, "USD", "Original desc", true);
        typeof(Tariff).GetProperty("Id")!.SetValue(existingTariff, tariffId);

        var command = new UpdateTariffCommand(tariffId, "Updated", "Updated desc", 20m, "EUR", false);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariff);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        existingTariff.Name.ShouldBe("Updated");
        existingTariff.Description.ShouldBe("Updated desc");
        existingTariff.Price.ShouldBe(20m);
        existingTariff.Currency.ShouldBe("EUR");
        existingTariff.IsActive.ShouldBeFalse();

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingTariff_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var command = new UpdateTariffCommand(tariffId, "Updated", null, 20m, "EUR", true);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tariff?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<EntityNotFoundException>(act);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var existingTariff = Tariff.Create("Original", 10m, "USD");
        typeof(Tariff).GetProperty("Id")!.SetValue(existingTariff, tariffId);

        var command = new UpdateTariffCommand(tariffId, "", null, 20m, "EUR", true);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariff);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}

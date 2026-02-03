using Cheetah.Admin.Modules.Clients.Application.Commands;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.Domain.Exceptions;
using Shouldly;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Commands;

public class DeleteTariffCommandHandlerTests
{
    private readonly Mock<ITariffRepository> _repositoryMock;
    private readonly DeleteTariffCommandHandler _handler;

    public DeleteTariffCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITariffRepository>();
        _handler = new DeleteTariffCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingTariff_ShouldDeleteTariff()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var existingTariff = Tariff.Create("Plan", 10m, "USD");
        typeof(Tariff).GetProperty("Id")!.SetValue(existingTariff, tariffId);

        var command = new DeleteTariffCommand(tariffId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTariff);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.Delete(existingTariff), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingTariff_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var command = new DeleteTariffCommand(tariffId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tariff?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<EntityNotFoundException>(act);
    }
}

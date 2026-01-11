using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Features.Application.Commands;
using Cheetah.Features.Domain.Entities;
using Cheetah.Features.Events;
using FluentAssertions;
using Moq;

namespace Cheetah.Features.Application.Tests.Commands;

public class CreateFeatureCommandHandlerTests
{
    private readonly Mock<IRepository<Feature, string>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateFeatureCommandHandler _handler;

    public CreateFeatureCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Feature, string>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateFeatureCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateFeature_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateFeatureCommand(
            "advanced-reporting",
            "Advanced Reporting",
            "Enable advanced reporting features",
            true,
            "Reporting"
        );

        Feature? capturedFeature = null;
        _repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Feature>(), It.IsAny<CancellationToken>()))
            .Callback<Feature, CancellationToken>((feature, _) => capturedFeature = feature)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().Be(command.Id);
        capturedFeature.Should().NotBeNull();
        capturedFeature!.Id.Should().Be(command.Id);
        capturedFeature.DisplayName.Should().Be(command.DisplayName);
        capturedFeature.Description.Should().Be(command.Description);
        capturedFeature.IsEnabledByDefault.Should().Be(command.IsEnabledByDefault);
        capturedFeature.Group.Should().Be(command.Group);

        _repositoryMock.Verify(x => x.InsertAsync(It.IsAny<Feature>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishFeatureCreatedEvent_WhenFeatureIsCreated()
    {
        // Arrange
        var command = new CreateFeatureCommand(
            "test-feature",
            "Test Feature",
            "Test description",
            false,
            null
        );

        _repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Feature>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventBusMock.Verify(
            x => x.PublishAsync(
                It.Is<IEvent>(e =>
                    (e as FeatureCreatedEvent) != null &&
                    (e as FeatureCreatedEvent)!.FeatureId == command.Id &&
                    (e as FeatureCreatedEvent)!.DisplayName == command.DisplayName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFeatureId_WhenFeatureIsCreated()
    {
        // Arrange
        var command = new CreateFeatureCommand(
            "return-test-feature",
            "Return Test Feature",
            null,
            false,
            null
        );

        _repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Feature>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().Be(command.Id);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateFeature_WithMinimalData()
    {
        // Arrange
        var command = new CreateFeatureCommand(
            "minimal-feature",
            "Minimal Feature",
            null,
            false,
            null
        );

        Feature? capturedFeature = null;
        _repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Feature>(), It.IsAny<CancellationToken>()))
            .Callback<Feature, CancellationToken>((feature, _) => capturedFeature = feature)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedFeature.Should().NotBeNull();
        capturedFeature!.Id.Should().Be(command.Id);
        capturedFeature.DisplayName.Should().Be(command.DisplayName);
        capturedFeature.Description.Should().BeNull();
        capturedFeature.IsEnabledByDefault.Should().BeFalse();
        capturedFeature.Group.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateFeature_WithEnabledByDefault()
    {
        // Arrange
        var command = new CreateFeatureCommand(
            "enabled-feature",
            "Enabled Feature",
            "Auto-enabled feature",
            true,
            "Core"
        );

        Feature? capturedFeature = null;
        _repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Feature>(), It.IsAny<CancellationToken>()))
            .Callback<Feature, CancellationToken>((feature, _) => capturedFeature = feature)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedFeature.Should().NotBeNull();
        capturedFeature!.IsEnabledByDefault.Should().BeTrue();
    }
}

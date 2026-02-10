using __Prefix__.ModuleName.Application.Commands;
using __Prefix__.ModuleName.Domain;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Shouldly;
using Moq;

namespace __Prefix__.ModuleName.Application.Tests.Commands;

public class CreateSampleEntityCommandHandlerTests
{
    private readonly Mock<IRepository<SampleEntity, Guid>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateSampleEntityCommandHandler _handler;

    public CreateSampleEntityCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<SampleEntity, Guid>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateSampleEntityCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateSampleEntityCommand("Test Entity", "Test Description");
        SampleEntity? capturedEntity = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<SampleEntity>()))
            .Callback<SampleEntity>(e => capturedEntity = e);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedEntity.ShouldNotBeNull();
        capturedEntity!.Name.ShouldBe("Test Entity");
        capturedEntity.Description.ShouldBe("Test Description");

        _repositoryMock.Verify(r => r.Add(It.IsAny<SampleEntity>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateSampleEntityCommand("", "Description");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}

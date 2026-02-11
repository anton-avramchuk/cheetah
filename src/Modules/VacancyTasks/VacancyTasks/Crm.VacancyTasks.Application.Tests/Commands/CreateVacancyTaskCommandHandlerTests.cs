using Crm.VacancyTasks.Application.Commands;
using Crm.VacancyTasks.Domain;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Shouldly;
using Moq;

namespace Crm.VacancyTasks.Application.Tests.Commands;

public class CreateVacancyTaskCommandHandlerTests
{
    private readonly Mock<IRepository<VacancyTask, Guid>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateVacancyTaskCommandHandler _handler;

    public CreateVacancyTaskCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<VacancyTask, Guid>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateVacancyTaskCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateVacancyTaskCommand("Test Entity", "Test Description");
        VacancyTask? capturedEntity = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<VacancyTask>()))
            .Callback<VacancyTask>(e => capturedEntity = e);

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

        _repositoryMock.Verify(r => r.Add(It.IsAny<VacancyTask>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateVacancyTaskCommand("", "Description");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}
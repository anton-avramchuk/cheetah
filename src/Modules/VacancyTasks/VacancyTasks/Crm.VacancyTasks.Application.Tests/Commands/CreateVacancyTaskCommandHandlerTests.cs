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
        var vacancyId = Guid.NewGuid();
        var stateId = Guid.NewGuid();
        var command = new CreateVacancyTaskCommand("Test Task", vacancyId, stateId, "Test Description", null, null, null);
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
        capturedEntity!.Title.ShouldBe("Test Task");
        capturedEntity.Description.ShouldBe("Test Description");
        capturedEntity.VacancyId.ShouldBe(vacancyId);
        capturedEntity.StateId.ShouldBe(stateId);

        _repositoryMock.Verify(r => r.Add(It.IsAny<VacancyTask>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateVacancyTaskCommand("", Guid.NewGuid(), Guid.NewGuid(), "Description", null, null, null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}

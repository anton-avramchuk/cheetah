using Cheetah.Core.DataAccess.Abstractions;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Domain;
using Cheetah.Core.Events;
using Shouldly;
using Moq;

namespace Crm.Recruitment.Application.Tests.Commands;

public class CreateVacancyCommandHandlerTests
{
    private readonly Mock<IRepository<Vacancy, Guid>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateVacancyCommandHandler _handler;

    public CreateVacancyCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Vacancy, Guid>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateVacancyCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateVacancyCommand("Test Entity", "Test Description");
        Vacancy? capturedEntity = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Vacancy>()))
            .Callback<Vacancy>(e => capturedEntity = e);

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

        _repositoryMock.Verify(r => r.Add(It.IsAny<Vacancy>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateVacancyCommand("", "Description");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}

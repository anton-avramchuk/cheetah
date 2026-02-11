using Crm.Candidates.Application.Commands;
using Crm.Candidates.Domain;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Shouldly;
using Moq;

namespace Crm.Candidates.Application.Tests.Commands;

public class CreateCandidateCommandHandlerTests
{
    private readonly Mock<IRepository<Candidate, Guid>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateCandidateCommandHandler _handler;

    public CreateCandidateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Candidate, Guid>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateCandidateCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateCandidateCommand("John", "Doe", "john@test.com", null, null, null, null, null, null);
        Candidate? capturedEntity = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Candidate>()))
            .Callback<Candidate>(e => capturedEntity = e);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedEntity.ShouldNotBeNull();
        capturedEntity!.FirstName.ShouldBe("John");
        capturedEntity.LastName.ShouldBe("Doe");
        capturedEntity.Email.ShouldBe("john@test.com");

        _repositoryMock.Verify(r => r.Add(It.IsAny<Candidate>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateCandidateCommand("", "Doe", null, null, null, null, null, null, null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}

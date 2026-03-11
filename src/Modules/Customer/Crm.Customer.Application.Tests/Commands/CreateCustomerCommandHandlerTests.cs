using Crm.Customer.Application.Commands;
using Crm.Customer.Domain;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Shouldly;
using Moq;

namespace Crm.Customer.Application.Tests.Commands;

public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<IRepository<Customer, Guid>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Customer, Guid>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateCustomerCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateCustomerCommand("Test Entity", "Test Description");
        Customer? capturedEntity = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Customer>()))
            .Callback<Customer>(e => capturedEntity = e);

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

        _repositoryMock.Verify(r => r.Add(It.IsAny<Customer>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateCustomerCommand("", "Description");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
    }
}
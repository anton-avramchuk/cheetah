using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Tenants.Application.Commands;
using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Domain.Events;
using FluentAssertions;
using Moq;

namespace Cheetah.Tenants.Application.Tests.Commands;

public class CreateTenantCommandHandlerTests
{
    private readonly Mock<IRepository<Tenant, Guid>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreateTenantCommandHandler _handler;

    public CreateTenantCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Tenant, Guid>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreateTenantCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateTenant_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Tenant", "test-subdomain");
        Tenant? capturedTenant = null;

        _repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .Callback<Tenant, CancellationToken>((tenant, _) => capturedTenant = tenant)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedTenant.Should().NotBeNull();
        capturedTenant!.Name.Should().Be("Test Tenant");
        capturedTenant.Subdomain.Should().Be("test-subdomain");

        _repositoryMock.Verify(x => x.InsertAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishTenantCreatedEvent_WhenTenantIsCreated()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Tenant", "test-subdomain");

        _repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventBusMock.Verify(
            x => x.PublishAsync(
                It.Is<IEvent>(e =>
                    (e as TenantCreatedEvent) != null &&
                    (e as TenantCreatedEvent)!.TenantName == "Test Tenant" &&
                    (e as TenantCreatedEvent)!.Subdomain == "test-subdomain"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTenantId_WhenTenantIsCreated()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Tenant", null);

        _repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateTenantWithoutSubdomain_WhenSubdomainIsNull()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Tenant", null);
        Tenant? capturedTenant = null;

        _repositoryMock
            .Setup(x => x.InsertAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .Callback<Tenant, CancellationToken>((tenant, _) => capturedTenant = tenant)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedTenant.Should().NotBeNull();
        capturedTenant!.Name.Should().Be("Test Tenant");
        capturedTenant.Subdomain.Should().BeNull();
    }
}

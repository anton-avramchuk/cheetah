using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Tenants.Application.Commands;
using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Domain.Events;
using FluentAssertions;
using Moq;

namespace Cheetah.Tenants.Application.Tests.Commands;

public class DeactivateTenantCommandHandlerTests
{
    private readonly Mock<IRepository<Tenant, Guid>> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly DeactivateTenantCommandHandler _handler;

    public DeactivateTenantCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Tenant, Guid>>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new DeactivateTenantCommandHandler(_repositoryMock.Object, _eventBusMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldDeactivateTenant_WhenTenantExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant", "test-subdomain");
        tenant.ClearDomainEvents();
        tenant.Activate();
        tenant.ClearDomainEvents();

        var command = new DeactivateTenantCommand(tenantId);

        _repositoryMock
            .Setup(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        tenant.IsActive.Should().BeFalse();
        _repositoryMock.Verify(x => x.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishTenantDeactivatedEvent_WhenTenantIsDeactivated()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("Test Tenant", "test-subdomain");
        tenant.ClearDomainEvents();
        tenant.Activate();
        tenant.ClearDomainEvents();

        var command = new DeactivateTenantCommand(tenantId);

        _repositoryMock
            .Setup(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventBusMock.Verify(
            x => x.PublishAsync(
                It.Is<IEvent>(e =>
                    (e as TenantDeactivatedEvent) != null &&
                    (e as TenantDeactivatedEvent)!.TenantId == tenant.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenTenantNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new DeactivateTenantCommand(tenantId);

        _repositoryMock
            .Setup(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Tenant with ID {tenantId} not found");
    }
}

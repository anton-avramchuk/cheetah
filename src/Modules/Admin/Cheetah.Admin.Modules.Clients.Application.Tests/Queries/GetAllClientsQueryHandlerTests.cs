using Cheetah.Admin.Modules.Clients.Application.Queries;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.Specification;
using FluentAssertions;
using Moq;

namespace Cheetah.Admin.Modules.Clients.Application.Tests.Queries;

public class GetAllClientsQueryHandlerTests
{
    private readonly Mock<IClientRepository> _repositoryMock;
    private readonly GetAllClientsQueryHandler _handler;

    public GetAllClientsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
        _handler = new GetAllClientsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithClients_ShouldReturnClientModels()
    {
        // Arrange
        var clients = new List<Client>
        {
            CreateClient(Guid.NewGuid(), "Client 1", "Description 1"),
            CreateClient(Guid.NewGuid(), "Client 2", "Description 2"),
            CreateClient(Guid.NewGuid(), "Client 3", null)
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Client>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clients);

        var query = new GetAllClientsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Client 1");
        result[1].Name.Should().Be("Client 2");
        result[2].Name.Should().Be("Client 3");
    }

    [Fact]
    public async Task HandleAsync_WithNoClients_ShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Client>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client>());

        var query = new GetAllClientsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapTenantCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var client = CreateClient(Guid.NewGuid(), "Client 1", "Description", tenantId);

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Client>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client> { client });

        var query = new GetAllClientsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Tenant.Should().NotBeNull();
        result[0].Tenant!.Id.Should().Be(tenantId);
    }

    [Fact]
    public async Task HandleAsync_WithNullTenantId_ShouldMapToNullTenant()
    {
        // Arrange
        var client = CreateClient(Guid.NewGuid(), "Client 1", "Description", null);

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Client>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client> { client });

        var query = new GetAllClientsQuery();

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().HaveCount(1);
        result[0].Tenant.Should().BeNull();
    }

    private static Client CreateClient(Guid id, string name, string? description, Guid? tenantId = null)
    {
        var client = Client.Create(name, description, tenantId);
        typeof(Client).GetProperty("Id")!.SetValue(client, id);
        return client;
    }
}

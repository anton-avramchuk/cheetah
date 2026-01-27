using Cheetah.Core.Domain;

namespace Cheetah.Admin.Modules.Clients.Domain;

public class Client : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public Guid? TenantId { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private Client() { } // For EF Core

    public static Client Create(string name, string? description = null, Guid? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            TenantId = tenantId
        };

        // TODO: Add domain event when Events project is created
        // client.AddDomainEvent(new ClientCreatedEvent(client.Id, client.Name));

        return client;
    }

    public void Update(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description;

        // TODO: Add domain event when Events project is created
        // AddDomainEvent(new ClientUpdatedEvent(Id, Name));
    }

    public void AssignToTenant(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
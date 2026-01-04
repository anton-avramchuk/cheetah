using Cheetah.Core.Tenants.Domain;
using Cheetah.Core.Tenants.Events;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.EntityFramework.Tenants;

public abstract class CrmTenantsDbContext<TContext, TTenant, TTenantCreatedEvent, TTenantUpdatedEvent, TTenantDeactivatedEvent, TTenantActivatedEvent>
    : CrmDbContext<TContext>
    where TContext : CrmTenantsDbContext<TContext, TTenant, TTenantCreatedEvent, TTenantUpdatedEvent, TTenantDeactivatedEvent, TTenantActivatedEvent>
    where TTenant : TenantEntity<TTenantCreatedEvent, TTenantUpdatedEvent, TTenantDeactivatedEvent, TTenantActivatedEvent>
    where TTenantCreatedEvent : TenantCreatedEvent
    where TTenantUpdatedEvent : TenantUpdatedEvent
    where TTenantDeactivatedEvent : TenantDeactivatedEvent
    where TTenantActivatedEvent : TenantActivatedEvent
{
    public DbSet<TTenant> Tenants => Set<TTenant>();

    protected CrmTenantsDbContext(DbContextOptions<TContext> options) : base(options)
    {
    }
}
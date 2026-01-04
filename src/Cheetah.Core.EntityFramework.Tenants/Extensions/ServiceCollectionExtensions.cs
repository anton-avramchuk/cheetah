using Microsoft.Extensions.DependencyInjection;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.Tenants.Domain;
using Cheetah.Core.Tenants.Events;

namespace Cheetah.Core.EntityFramework.Tenants.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenantsDbContext<TDbContext, TTenant, TTenantCreatedEvent, TTenantUpdatedEvent, TTenantDeactivatedEvent, TTenantActivatedEvent>(
        this IServiceCollection services)
        where TDbContext : CrmTenantsDbContext<TDbContext, TTenant, TTenantCreatedEvent, TTenantUpdatedEvent, TTenantDeactivatedEvent, TTenantActivatedEvent>
        where TTenant : TenantEntity<TTenantCreatedEvent, TTenantUpdatedEvent, TTenantDeactivatedEvent, TTenantActivatedEvent>
        where TTenantCreatedEvent : TenantCreatedEvent
        where TTenantUpdatedEvent : TenantUpdatedEvent
        where TTenantDeactivatedEvent : TenantDeactivatedEvent
        where TTenantActivatedEvent : TenantActivatedEvent
    {
        services.AddApplicationDbContext<TDbContext>();

        return services;
    }
}
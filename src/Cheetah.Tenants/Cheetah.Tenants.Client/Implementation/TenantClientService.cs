using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Application.Services;
using Cheetah.Tenants.Client.Interfaces;
using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Contracts.ViewModels;

namespace Cheetah.Tenants.Client.Implementation;

/// <summary>
/// Direct implementation of tenant client service.
/// Uses IDispatcher for queries and ITenantStore for connection strings.
/// Can be replaced with HTTP client implementation for microservices.
/// </summary>
[Export(LifetimeType.Scoped, typeof(ITenantClientService))]
public class TenantClientService : ITenantClientService
{
    private readonly IDispatcher _dispatcher;
    private readonly ITenantStore _tenantStore;

    public TenantClientService(IDispatcher dispatcher, ITenantStore tenantStore)
    {
        _dispatcher = dispatcher;
        _tenantStore = tenantStore;
    }

    public async ValueTask<TenantViewModel?> GetByIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        var query = new GetTenantByIdQuery(tenantId);
        var tenant = await _dispatcher.QueryAsync<GetTenantByIdQuery, Tenant?>(query, ct);
        return MapToViewModel(tenant);
    }

    public async ValueTask<TenantViewModel?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default)
    {
        var query = new GetTenantBySubdomainQuery(subdomain);
        var tenant = await _dispatcher.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(query, ct);
        return MapToViewModel(tenant);
    }

    public async ValueTask<List<TenantViewModel>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var query = new GetAllTenantsQuery();
        var tenants = await _dispatcher.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(query, ct);

        return tenants
            .Where(t => t.IsActive)
            .Select(MapToViewModel)
            .Where(vm => vm is not null)
            .Select(vm => vm!)
            .ToList();
    }

    public async ValueTask<string?> GetConnectionStringAsync(Guid tenantId, string name = "Default", CancellationToken ct = default)
    {
        var tenant = await _tenantStore.FindByIdAsync(tenantId, ct);
        if (tenant is null)
            return null;

        var connectionString = tenant.ConnectionStrings.FirstOrDefault(cs => cs.Name == name);
        return connectionString?.ConnectionString;
    }

    public async ValueTask<bool> IsActiveAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.FindByIdAsync(tenantId, ct);
        return tenant?.IsActive ?? false;
    }

    public async ValueTask<bool> ExistsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantStore.FindByIdAsync(tenantId, ct);
        return tenant is not null;
    }

    private static TenantViewModel? MapToViewModel(Tenant? tenant)
    {
        if (tenant is null)
            return null;

        return new TenantViewModel
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt,
            ConnectionStrings = tenant.ConnectionStrings
                .Select(cs => new TenantConnectionStringViewModel
                {
                    Id = cs.Id,
                    Name = cs.Name,
                    ConnectionString = cs.ConnectionString,
                    IsDefault = cs.IsDefault
                })
                .ToList()
        };
    }
}

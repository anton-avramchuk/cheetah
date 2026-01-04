using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Tenants;
using Cheetah.Core.Tenants.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cheetah.Identity.DataAccess;

/// <summary>
/// Provider for creating tenant-specific Identity DbContext instances
/// Registered as singleton to provide factory capability
/// </summary>
[Export(LifetimeType.Singleton, typeof(ITenantBasedDbContext<TenantCreatedEvent>))]
public class IdentityTenantDbContextProvider : ITenantBasedDbContext<TenantCreatedEvent>
{
    public string ModuleName => "Identity";

    public DatabaseFacade Database => throw new NotSupportedException(
        "This is a factory provider. Use CreateForTenant to get a DbContext instance.");

    public ITenantBasedDbContext<TenantCreatedEvent> CreateForTenant(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        var dbContext = new IdentityDbContext(optionsBuilder.Options);
        return new TenantIdentityDbContext(dbContext);
    }

    public void Dispose()
    {
        // This is a factory provider, nothing to dispose
    }

    public ValueTask DisposeAsync()
    {
        // This is a factory provider, nothing to dispose
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Wrapper for IdentityDbContext to implement ITenantBasedDbContext
/// </summary>
internal class TenantIdentityDbContext : ITenantBasedDbContext<TenantCreatedEvent>
{
    private readonly IdentityDbContext _dbContext;

    public TenantIdentityDbContext(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string ModuleName => "Identity";
    public DatabaseFacade Database => _dbContext.Database;

    public ITenantBasedDbContext<TenantCreatedEvent> CreateForTenant(string connectionString)
    {
        throw new NotSupportedException("This instance is already tenant-specific. Use the provider to create new instances.");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
}

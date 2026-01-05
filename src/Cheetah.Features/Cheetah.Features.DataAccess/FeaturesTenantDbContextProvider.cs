using Cheetah.Core.EntityFramework.Tenants;
using Cheetah.Tenants.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cheetah.Features.DataAccess;

/// <summary>
/// Provider for creating tenant-specific Features DbContext instances
/// </summary>
public class FeaturesTenantDbContextProvider : ITenantBasedDbContext<TenantCreatedEvent>
{
    private FeaturesDbContext? _context;

    public string ModuleName => "Features";

    public DatabaseFacade Database => _context?.Database
        ?? throw new InvalidOperationException("DbContext not initialized. Call CreateForTenant first.");

    public ITenantBasedDbContext<TenantCreatedEvent> CreateForTenant(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FeaturesDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new FeaturesTenantDbContextProvider
        {
            _context = new FeaturesDbContext(optionsBuilder.Options)
        };
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        if (_context != null)
        {
            return _context.DisposeAsync();
        }
        return ValueTask.CompletedTask;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheetah.Tenants.DataAccess;

/// <summary>
/// Design-time factory for creating TenantsDbContext instances for EF Core migrations
/// </summary>
public class TenantsDbContextFactory : IDesignTimeDbContextFactory<TenantsDbContext>
{
    public TenantsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantsDbContext>();

        // Use connection string for design-time (migrations)
        // This will be overridden at runtime by the actual configuration
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=cheetah_tenants;Username=cheetah;Password=cheetah123");

        return new TenantsDbContext(optionsBuilder.Options);
    }
}

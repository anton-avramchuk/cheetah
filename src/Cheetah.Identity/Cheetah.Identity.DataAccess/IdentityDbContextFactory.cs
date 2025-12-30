using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheetah.Identity.DataAccess;

/// <summary>
/// Design-time factory for IdentityDbContext (for migrations)
/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();

        // Use a default connection string for migrations
        // This will be overridden at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=CheetahIdentity;Username=postgres;Password=postgres");

        return new IdentityDbContext(optionsBuilder.Options);
    }
}

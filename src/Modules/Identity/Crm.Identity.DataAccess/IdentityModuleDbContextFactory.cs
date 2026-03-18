using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Identity.DataAccess;

public class IdentityModuleDbContextFactory : IDesignTimeDbContextFactory<IdentityModuleDbContext>
{
    public IdentityModuleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityModuleDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=identity;Username=postgres;Password=postgres");

        return new IdentityModuleDbContext(optionsBuilder.Options);
    }
}

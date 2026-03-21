using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppName.Identity.DataAccess;

public class IdentityModuleDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
#if (databaseType == "postgres")
        optionsBuilder.UseNpgsql("Host=localhost;Database=AppName_identity;Username=postgres;Password=postgres");
#else
        optionsBuilder.UseSqlServer("Server=localhost;Database=AppName_identity;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True");
#endif

        return new IdentityDbContext(optionsBuilder.Options);
    }
}

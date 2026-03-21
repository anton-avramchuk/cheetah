using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppName.DataAccess;

public class AppNameDbContextFactory : IDesignTimeDbContextFactory<AppNameDbContext>
{
    public AppNameDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppNameDbContext>();
#if (databaseType == "postgres")
        optionsBuilder.UseNpgsql("Host=localhost;Database=appname;Username=postgres;Password=postgres");
#else
        optionsBuilder.UseSqlServer("Server=localhost;Database=appname;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True");
#endif

        return new AppNameDbContext(optionsBuilder.Options);
    }
}

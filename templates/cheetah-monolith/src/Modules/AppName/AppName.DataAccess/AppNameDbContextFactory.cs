using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppName.DataAccess;

public class AppNameDbContextFactory : IDesignTimeDbContextFactory<AppNameDbContext>
{
    public AppNameDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppNameDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=appname;Username=postgres;Password=postgres");

        return new AppNameDbContext(optionsBuilder.Options);
    }
}

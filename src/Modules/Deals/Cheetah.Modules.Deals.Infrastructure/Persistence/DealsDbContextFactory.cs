using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheetah.Modules.Deals.Infrastructure.Persistence;

/// <summary>Design-time фабрика для <c>dotnet ef migrations</c>.</summary>
public class DealsDbContextFactory : IDesignTimeDbContextFactory<DealsDbContext>
{
    public DealsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DealsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=deals;Username=postgres;Password=postgres");
        return new DealsDbContext(optionsBuilder.Options);
    }
}

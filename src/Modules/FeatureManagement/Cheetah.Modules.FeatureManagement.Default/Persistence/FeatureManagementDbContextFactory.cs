using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheetah.Modules.FeatureManagement.Default.Persistence;

/// <summary>Design-time фабрика для <c>dotnet ef migrations</c>.</summary>
public class FeatureManagementDbContextFactory : IDesignTimeDbContextFactory<FeatureManagementDbContext>
{
    public FeatureManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FeatureManagementDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=features;Username=postgres;Password=postgres");
        return new FeatureManagementDbContext(optionsBuilder.Options);
    }
}

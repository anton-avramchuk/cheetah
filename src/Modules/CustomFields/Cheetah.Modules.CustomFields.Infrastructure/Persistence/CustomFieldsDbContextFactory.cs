using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheetah.Modules.CustomFields.Infrastructure.Persistence;

/// <summary>Design-time фабрика для <c>dotnet ef migrations</c>.</summary>
public class CustomFieldsDbContextFactory : IDesignTimeDbContextFactory<CustomFieldsDbContext>
{
    public CustomFieldsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CustomFieldsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=custom_fields;Username=postgres;Password=postgres");
        return new CustomFieldsDbContext(optionsBuilder.Options);
    }
}

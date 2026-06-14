using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheetah.Modules.Tags.Infrastructure.Persistence;

/// <summary>Design-time фабрика для <c>dotnet ef migrations</c>.</summary>
public class TagsDbContextFactory : IDesignTimeDbContextFactory<TagsDbContext>
{
    public TagsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TagsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=tags;Username=postgres;Password=postgres");
        return new TagsDbContext(optionsBuilder.Options);
    }
}

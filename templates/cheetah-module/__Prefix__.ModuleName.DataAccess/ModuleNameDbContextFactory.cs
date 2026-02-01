using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace __Prefix__.ModuleName.DataAccess;

public class ModuleNameDbContextFactory : IDesignTimeDbContextFactory<ModuleNameDbContext>
{
    public ModuleNameDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ModuleNameDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=moduleschema;Username=postgres;Password=postgres");

        return new ModuleNameDbContext(optionsBuilder.Options);
    }
}

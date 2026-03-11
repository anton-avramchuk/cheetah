using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.MasterData.DataAccess;

public class MasterDataDbContextFactory : IDesignTimeDbContextFactory<MasterDataDbContext>
{
    public MasterDataDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasterDataDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=masterdata;Username=postgres;Password=postgres");

        return new MasterDataDbContext(optionsBuilder.Options);
    }
}
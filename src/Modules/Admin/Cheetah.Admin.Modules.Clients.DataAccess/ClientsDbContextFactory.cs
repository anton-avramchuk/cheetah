using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheetah.Admin.Modules.Clients.DataAccess;

public class ClientsDbContextFactory : IDesignTimeDbContextFactory<ClientsDbContext>
{
    public ClientsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClientsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=cheetah_admin;Username=postgres;Password=postgres");

        return new ClientsDbContext(optionsBuilder.Options);
    }
}

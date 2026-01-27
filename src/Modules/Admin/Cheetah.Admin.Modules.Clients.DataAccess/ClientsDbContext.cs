using Cheetah.Admin.Modules.Clients.DataAccess.Configurations;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Admin.Modules.Clients.DataAccess;

public class ClientsDbContext(DbContextOptions<ClientsDbContext> options) : CrmDbContext<ClientsDbContext>(options)
{
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ClientConfiguration());
    }
}
using Cheetah.Admin.Modules.Clients.DataAccess.Configurations;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Admin.Modules.Clients.DataAccess;

[ConnectionStringName("AdminDb")]
public class ClientsDbContext(DbContextOptions<ClientsDbContext> options) : CrmDbContext<ClientsDbContext>(options)
{
    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Tariff> Tariffs => Set<Tariff>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new TariffConfiguration());
    }
}
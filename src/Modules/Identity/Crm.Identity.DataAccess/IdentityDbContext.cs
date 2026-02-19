using Cheetah.Core.EntityFramework;
using Crm.Identity.DataAccess.Configurations;
using Crm.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.DataAccess;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : CrmDbContext<IdentityDbContext>(options)
{
    public DbSet<UserIdentity> SampleEntities => Set<UserIdentity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserIdentityConfiguration());
    }
}
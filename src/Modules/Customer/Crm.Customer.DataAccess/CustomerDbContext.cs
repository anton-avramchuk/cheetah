using Cheetah.Core.EntityFramework;
using Crm.Customer.DataAccess.Configurations;
using CustomerEntity = global::Crm.Customer.Domain.Customer;
using Microsoft.EntityFrameworkCore;

namespace Crm.Customer.DataAccess;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options)
    : CrmDbContext<CustomerDbContext>(options)
{
    public DbSet<CustomerEntity> SampleEntities => Set<CustomerEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}

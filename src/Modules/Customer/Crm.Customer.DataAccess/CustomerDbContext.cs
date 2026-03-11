using Cheetah.Core.EntityFramework;
using Crm.Customer.DataAccess.Configurations;
using Crm.Customer.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Customer.DataAccess;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options)
    : CrmDbContext<CustomerDbContext>(options)
{
    public DbSet<Customer> SampleEntities => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}
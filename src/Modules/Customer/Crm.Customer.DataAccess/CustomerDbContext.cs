using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Crm.Customer.DataAccess.Configurations;
using Crm.Customer.Domain;
using CustomerEntity = global::Crm.Customer.Domain.Customer;
using Microsoft.EntityFrameworkCore;

namespace Crm.Customer.DataAccess;

[ConnectionStringName("Customer")]
public class CustomerDbContext(DbContextOptions<CustomerDbContext> options)
    : CrmDbContext<CustomerDbContext>(options)
{
    public DbSet<CustomerEntity> SampleEntities => Set<CustomerEntity>();
    public DbSet<CustomerIndustry> CustomerIndustries => Set<CustomerIndustry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerIndustryConfiguration());
    }
}

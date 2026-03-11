using Cheetah.Core.EntityFramework.Configuration;
using CustomerEntity = global::Crm.Customer.Domain.Customer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Customer.DataAccess.Configurations;

public class CustomerConfigurationOptions : AggregateRootConfigurationOptions<CustomerEntity, Guid>
{
    public override string Schema => "customer";
}

public class CustomerConfiguration : AggregateRootConfiguration<CustomerEntity, Guid, CustomerConfigurationOptions>
{
    protected override CustomerConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}

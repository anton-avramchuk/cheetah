using Cheetah.Core.EntityFramework.Configuration;
using Crm.Customer.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Customer.DataAccess.Configurations;

public class CustomerConfigurationOptions : AggregateRootConfigurationOptions<Customer, Guid>
{
    public override string Schema => "customer";
}

public class CustomerConfiguration : AggregateRootConfiguration<Customer, Guid, CustomerConfigurationOptions>
{
    protected override CustomerConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<Customer> builder)
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
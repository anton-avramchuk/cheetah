using Cheetah.Core.EntityFramework.Configuration;
using Crm.Customer.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Customer.DataAccess.Configurations;

public class CustomerIndustryConfigurationOptions : EntityConfigurationOptions<CustomerIndustry, Guid>
{
    public override string Schema => "customer";
}

public class CustomerIndustryConfiguration
    : EntityConfiguration<CustomerIndustry, Guid, CustomerIndustryConfigurationOptions>
{
    protected override CustomerIndustryConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CustomerIndustry> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}

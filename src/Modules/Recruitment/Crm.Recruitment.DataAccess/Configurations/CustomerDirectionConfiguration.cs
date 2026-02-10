using Cheetah.Core.EntityFramework.Configuration;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Recruitment.DataAccess.Configurations;

public class CustomerDirectionConfigurationOptions : EntityConfigurationOptions<CustomerDirection, Guid>
{
    public override string Schema => "recruitment";
}

public class CustomerDirectionConfiguration : EntityConfiguration<CustomerDirection, Guid, CustomerDirectionConfigurationOptions>
{
    protected override CustomerDirectionConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CustomerDirection> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Navigation(x => x.Customers)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

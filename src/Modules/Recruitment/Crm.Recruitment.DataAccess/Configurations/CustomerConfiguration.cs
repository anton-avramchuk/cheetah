using Cheetah.Core.EntityFramework.Configuration;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Recruitment.DataAccess.Configurations;

public class CustomerConfigurationOptions : EntityConfigurationOptions<Customer, Guid>
{
    public override string Schema => "recruitment";
}

public class CustomerConfiguration : EntityConfiguration<Customer, Guid, CustomerConfigurationOptions>
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

        builder.HasOne(x => x.Direction)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.DirectionId)
            .IsRequired(false);

        builder.Navigation(x => x.Vacancies)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

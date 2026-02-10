using Cheetah.Core.EntityFramework.Configuration;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Recruitment.DataAccess.Configurations;

public class StackItemConfigurationOptions : EntityConfigurationOptions<StackItem, Guid>
{
    public override string Schema => "recruitment";
}

public class StackItemConfiguration : EntityConfiguration<StackItem, Guid, StackItemConfigurationOptions>
{
    protected override StackItemConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<StackItem> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Navigation(x => x.Vacancies)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

using Cheetah.Core.EntityFramework.Configuration;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.MasterData.DataAccess.Configurations;

public class StackItemConfigurationOptions : AggregateRootConfigurationOptions<StackItem, Guid>
{
    public override string Schema => "masterdata";
}

public class StackItemConfiguration : AggregateRootConfiguration<StackItem, Guid, StackItemConfigurationOptions>
{
    protected override StackItemConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<StackItem> builder)
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
using Cheetah.Core.EntityFramework.Configuration;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.MasterData.DataAccess.Configurations;

public class IndustryConfigurationOptions : EntityConfigurationOptions<Industry, Guid>
{
    public override string Schema => Constants.SchemaName;
}

public class IndustryConfiguration : EntityConfiguration<Industry, Guid, IndustryConfigurationOptions>
{
    protected override IndustryConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<Industry> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}

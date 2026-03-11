using Cheetah.Core.EntityFramework.Configuration;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.MasterData.DataAccess.Configurations;

public class PositionConfigurationOptions : EntityConfigurationOptions<Position, Guid>
{
    public override string Schema => Constants.SchemaName;
}

public class PositionConfiguration : EntityConfiguration<Position, Guid, PositionConfigurationOptions>
{
    protected override PositionConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<Position> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Grade)
            .IsRequired();
    }
}

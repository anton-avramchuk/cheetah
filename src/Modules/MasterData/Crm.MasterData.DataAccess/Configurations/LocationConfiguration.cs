using Cheetah.Core.EntityFramework.Configuration;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.MasterData.DataAccess.Configurations;

public class LocationConfigurationOptions : EntityConfigurationOptions<Location, Guid>
{
    public override string Schema => Constants.SchemaName;
}

public class LocationConfiguration : EntityConfiguration<Location, Guid, LocationConfigurationOptions>
{
    protected override LocationConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<Location> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Timezone)
            .IsRequired()
            .HasMaxLength(64);
    }
}

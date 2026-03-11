using Cheetah.Core.EntityFramework.Configuration;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.MasterData.DataAccess.Configurations;

public class WorkFormatConfigurationOptions : EntityConfigurationOptions<WorkFormat, Guid>
{
    public override string Schema => Constants.SchemaName;
}

public class WorkFormatConfiguration : EntityConfiguration<WorkFormat, Guid, WorkFormatConfigurationOptions>
{
    protected override WorkFormatConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<WorkFormat> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}

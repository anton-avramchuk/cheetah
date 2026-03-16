using Cheetah.Core.EntityFramework.Configuration;
using __Prefix__.ModuleName.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace __Prefix__.ModuleName.DataAccess.Configurations;

public class SampleEntityConfigurationOptions : AggregateRootConfigurationOptions<SampleEntity, Guid>
{
    public override string Schema => "moduleschema";
}

public class SampleEntityConfiguration : AggregateRootConfiguration<SampleEntity, Guid, SampleEntityConfigurationOptions>
{
    protected override SampleEntityConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<SampleEntity> builder)
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

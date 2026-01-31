using Cheetah.Core.EntityFramework.Configuration;
using Crm.Features.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Features.DataAccess.Configurations;


public class FeatureConfigurationOptions : AggregateRootConfigurationOptions<Feature, Guid>
{
    public override string Schema => "features";
}

public class FeatureConfiguration:AggregateRootConfiguration<Feature, Guid, FeatureConfigurationOptions>
{
    protected override FeatureConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<Feature> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
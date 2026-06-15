using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Deals.Infrastructure.Persistence.Configurations;

public class PipelineConfiguration : IEntityTypeConfiguration<Pipeline>
{
    public void Configure(EntityTypeBuilder<Pipeline> builder)
    {
        builder.ToTable("Pipelines", DealsConstants.Schema);
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.Name).HasMaxLength(DealsConstants.MaxNameLength).IsRequired();

        builder.HasMany(x => x.Stages)
            .WithOne()
            .HasForeignKey(s => s.PipelineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Stages)
            .HasField("_stages")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.IsDefault);
    }
}

public class PipelineStageConfiguration : IEntityTypeConfiguration<PipelineStage>
{
    public void Configure(EntityTypeBuilder<PipelineStage> builder)
    {
        builder.ToTable("PipelineStages", DealsConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(DealsConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>();

        builder.HasIndex(x => new { x.PipelineId, x.Order });
    }
}

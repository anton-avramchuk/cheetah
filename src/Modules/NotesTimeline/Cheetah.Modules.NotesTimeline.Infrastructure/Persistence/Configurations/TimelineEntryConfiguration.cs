using Cheetah.Modules.NotesTimeline.Domain.Entities;
using Cheetah.Modules.NotesTimeline.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.NotesTimeline.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF-конфигурация строки ленты (read-model). Индекс <c>(EntityType, EntityId, OccurredAt)</c> — под
/// курсорную пагинацию карточки; уникальный индекс по <c>SourceEventId</c> — анти-дубль при повторной
/// доставке события-источника.
/// </summary>
public sealed class TimelineEntryConfiguration : IEntityTypeConfiguration<TimelineEntry>
{
    public void Configure(EntityTypeBuilder<TimelineEntry> builder)
    {
        builder.ToTable(NotesTimelineConstants.DefaultTimelineTableName, NotesTimelineConstants.DefaultSchema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType).HasMaxLength(NotesTimelineConstants.MaxEntityTypeLength).IsRequired();
        builder.Property(x => x.Kind).HasMaxLength(NotesTimelineConstants.MaxKindLength).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(NotesTimelineConstants.MaxTitleLength).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb");
        builder.Property(x => x.SourceEventId).HasMaxLength(128);

        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAt });
        builder.HasIndex(x => x.SourceEventId).IsUnique();
    }
}

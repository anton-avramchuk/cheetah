using Cheetah.Modules.Notes.Domain.Entities;
using Cheetah.Modules.Notes.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Notes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация заметки: ключ, игнор доменных событий, общие колонки,
/// jsonb-коллекции упоминаний/вложений, индексы под горячие пути и query-filter мягкого удаления.
/// Наследник наследует её и добавляет свои поля/индексы через <see cref="ConfigureCustom"/> —
/// точка расширения схемы.
/// </summary>
public abstract class NoteConfigurationBase<TNote> : IEntityTypeConfiguration<TNote>
    where TNote : NoteBase
{
    protected virtual string TableName => NotesConstants.DefaultNotesTableName;
    protected virtual string Schema => NotesConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TNote> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType).HasMaxLength(NotesConstants.MaxEntityTypeLength).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(NotesConstants.MaxBodyLength).IsRequired();
        builder.Property(x => x.Attributes).HasColumnType("jsonb");

        builder.Property(x => x.Mentions)
            .HasField("_mentions")
            .HasColumnName("Mentions")
            .HasColumnType("jsonb")
            .HasConversion(GuidListJsonConverters.Converter, GuidListJsonConverters.Comparer);

        builder.Property(x => x.AttachmentFileIds)
            .HasField("_attachmentFileIds")
            .HasColumnName("AttachmentFileIds")
            .HasColumnType("jsonb")
            .HasConversion(GuidListJsonConverters.Converter, GuidListJsonConverters.Comparer);

        // Горячие пути: заметки сущности, закреплённые, «мои заметки», ветка треда.
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.PinnedAt);
        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.ParentNoteId);

        // Soft-delete скрыт по умолчанию.
        builder.HasQueryFilter(x => x.RemovedAt == null);

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TNote> builder)
    {
    }
}

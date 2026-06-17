using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.NotesTimeline.Domain.Entities;
using Cheetah.Modules.NotesTimeline.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.NotesTimeline.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.NotesTimeline.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля NotesTimeline. Хостит заметки
/// (<typeparamref name="TNote"/>) и строки ленты в одной БД. Наследник закрывает его конкретным типом
/// заметки:
/// <c>class AppNotesDbContext : NotesTimelineDbContextBase&lt;AppNotesDbContext, Note&gt;</c>
/// и поставляет конфигурацию заметки через <see cref="CreateNoteConfiguration"/>. Миграции —
/// у наследника. Имя подключения по умолчанию — <see cref="NotesTimelineConstants.ConnectionStringName"/>.
/// </summary>
[ConnectionStringName(NotesTimelineConstants.ConnectionStringName)]
public abstract class NotesTimelineDbContextBase<TContext, TNote> : CrmDbContext<TContext>
    where TContext : DbContext
    where TNote : NoteBase
{
    public DbSet<TNote> Notes => Set<TNote>();
    public DbSet<TimelineEntry> TimelineEntries => Set<TimelineEntry>();

    protected NotesTimelineDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(CreateNoteConfiguration());
        modelBuilder.ApplyConfiguration(new TimelineEntryConfiguration());
    }

    /// <summary>Конкретная конфигурация сущности заметки, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TNote> CreateNoteConfiguration();
}

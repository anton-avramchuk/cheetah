using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Notes.Domain.Entities;
using Cheetah.Modules.Notes.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Notes.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля Notes. Наследник закрывает его конкретным типом
/// заметки:
/// <c>class AppNotesDbContext : NotesDbContextBase&lt;AppNotesDbContext, Note&gt;</c>
/// и поставляет конфигурацию заметки через <see cref="CreateNoteConfiguration"/>. Миграции —
/// у наследника. Имя подключения по умолчанию — <see cref="NotesConstants.ConnectionStringName"/>.
/// </summary>
[ConnectionStringName(NotesConstants.ConnectionStringName)]
public abstract class NotesDbContextBase<TContext, TNote> : CrmDbContext<TContext>
    where TContext : DbContext
    where TNote : NoteBase
{
    public DbSet<TNote> Notes => Set<TNote>();

    protected NotesDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(CreateNoteConfiguration());
    }

    /// <summary>Конкретная конфигурация сущности заметки, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TNote> CreateNoteConfiguration();
}

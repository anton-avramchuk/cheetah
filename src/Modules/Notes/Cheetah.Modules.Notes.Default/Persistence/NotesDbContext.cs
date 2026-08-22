using Cheetah.Modules.Notes.Default.Entities;
using Cheetah.Modules.Notes.Default.Persistence.Configurations;
using Cheetah.Modules.Notes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Notes.Default.Persistence;

/// <summary>Конкретный DbContext «из коробки» поверх абстрактной базы шаблона.</summary>
public sealed class NotesDbContext : NotesDbContextBase<NotesDbContext, Note>
{
    public NotesDbContext(DbContextOptions<NotesDbContext> options) : base(options)
    {
    }

    protected override IEntityTypeConfiguration<Note> CreateNoteConfiguration() => new NoteConfiguration();
}

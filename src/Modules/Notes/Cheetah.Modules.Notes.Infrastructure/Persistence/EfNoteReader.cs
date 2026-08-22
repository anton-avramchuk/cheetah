using Cheetah.Modules.Notes.Domain.Abstractions;
using Cheetah.Modules.Notes.Domain.Entities;
using Cheetah.Modules.Notes.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Notes.Infrastructure.Persistence;

/// <summary>
/// EF-реализация <see cref="INoteReader{TNote}"/>: фильтр — доменная спецификация, сортировка и
/// страница — на стороне БД, чтение без трекинга (списки только читают). Soft-deleted отсекает
/// глобальный query-фильтр контекста. Сортировка завершается <c>Id</c>: метки времени одного
/// <c>SaveChanges</c> совпадают, без тай-брейкера страницы дублировали бы и теряли строки.
/// </summary>
public sealed class EfNoteReader<TContext, TNote> : INoteReader<TNote>
    where TContext : NotesDbContextBase<TContext, TNote>
    where TNote : NoteBase
{
    private readonly TContext _context;

    public EfNoteReader(TContext context) => _context = context;

    public async ValueTask<IReadOnlyList<TNote>> GetPageByEntityAsync(
        string entityType, Guid entityId, bool pinnedOnly, int skip, int take, CancellationToken ct = default)
    {
        var spec = pinnedOnly
            ? new PinnedNotesByEntitySpecification<TNote>(entityType, entityId).ToExpression()
            : new NotesByEntitySpecification<TNote>(entityType, entityId).ToExpression();

        return await _context.Notes
            .AsNoTracking()
            .Where(spec)
            .OrderByDescending(n => n.PinnedAt != null)
            .ThenByDescending(n => n.PinnedAt)
            .ThenByDescending(n => n.CreatedAt)
            .ThenBy(n => n.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async ValueTask<IReadOnlyList<TNote>> GetRepliesPageAsync(
        Guid parentNoteId, int skip, int take, CancellationToken ct = default)
    {
        return await _context.Notes
            .AsNoTracking()
            .Where(new NoteThreadSpecification<TNote>(parentNoteId).ToExpression())
            .OrderBy(n => n.CreatedAt)
            .ThenBy(n => n.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }
}

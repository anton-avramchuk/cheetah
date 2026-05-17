using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Cheetah.Audit;

/// <summary>
/// EF Core interceptor: на SavingChangesAsync собирает все Auditable-сущности из
/// ChangeTracker и отдаёт зарегистрированным IAuditSink. EfAuditSink добавляет
/// AuditEntry в тот же DbContext — они уезжают в БД одной транзакцией с агрегатом.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IEnumerable<IAuditSink> _sinks;
    private readonly IAuditUserAccessor _user;

    public AuditInterceptor(IEnumerable<IAuditSink> sinks, IAuditUserAccessor user)
    {
        _sinks = sinks;
        _user = user;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } ctx)
        {
            var entries = BuildEntries(ctx);
            if (entries.Count > 0)
            {
                foreach (var sink in _sinks)
                {
                    await sink.EmitAsync(entries, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private List<AuditEntry> BuildEntries(DbContext ctx)
    {
        var list = new List<AuditEntry>();
        var occurredAt = DateTimeOffset.UtcNow;

        foreach (var entry in ctx.ChangeTracker.Entries())
        {
            var clrType = entry.Entity.GetType();
            if (clrType.GetCustomAttribute<AuditableAttribute>(inherit: true) is null) continue;

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Modified => AuditAction.Updated,
                EntityState.Deleted => AuditAction.Deleted,
                _ => (AuditAction?)null
            };
            if (action is null) continue;

            var entityId = ResolveEntityId(entry);
            if (entityId is null) continue; // не получили ключ — пропускаем (например db-generated до save)

            var changes = AuditChangesBuilder.Build(entry, action.Value);

            list.Add(new AuditEntry
            {
                EntityType = clrType.FullName ?? clrType.Name,
                EntityId = entityId,
                Action = action.Value,
                Changes = changes,
                OccurredAt = occurredAt,
                UserId = _user.UserId,
                UserName = _user.UserName,
                TenantId = _user.TenantId,
                CorrelationId = _user.CorrelationId
            });
        }

        return list;
    }

    private static string? ResolveEntityId(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return null;

        var parts = new List<string>(key.Properties.Count);
        foreach (var pkProp in key.Properties)
        {
            var value = entry.Property(pkProp.Name).CurrentValue;
            if (value is null) return null; // PK не задан
            parts.Add(value.ToString() ?? string.Empty);
        }
        return parts.Count == 1 ? parts[0] : string.Join("|", parts);
    }
}

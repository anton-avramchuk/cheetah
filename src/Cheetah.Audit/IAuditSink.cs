namespace Cheetah.Audit;

/// <summary>
/// Куда улетают audit-записи. Все зарегистрированные sinks вызываются последовательно
/// из AuditInterceptor.SavingChangesAsync — ДО фактического SaveChanges. EF-реализация
/// этим пользуется, чтобы попасть в ту же транзакцию что и бизнес-изменения.
/// </summary>
public interface IAuditSink
{
    ValueTask EmitAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default);
}

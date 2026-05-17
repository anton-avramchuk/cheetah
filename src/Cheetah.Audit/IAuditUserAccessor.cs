namespace Cheetah.Audit;

/// <summary>
/// Источник контекста "кто действует" в текущем запросе. Реализуется обычно поверх
/// IHttpContextAccessor (для web) или AsyncLocal (для фоновых задач).
/// </summary>
public interface IAuditUserAccessor
{
    string? UserId { get; }
    string? UserName { get; }
    string? TenantId { get; }
    string? CorrelationId { get; }
}

/// <summary>
/// Заглушка для dev/test и сценариев без аутентификации. Все поля null.
/// </summary>
public sealed class NullAuditUserAccessor : IAuditUserAccessor
{
    public static readonly NullAuditUserAccessor Instance = new();

    public string? UserId => null;
    public string? UserName => null;
    public string? TenantId => null;
    public string? CorrelationId => null;
}

namespace Cheetah.Audit;

/// <summary>
/// Помечает сущность как подлежащую аудиту. AuditInterceptor создаст AuditEntry
/// при Add/Update/Delete только для типов с этим атрибутом.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class AuditableAttribute : Attribute
{
}

/// <summary>
/// Исключает свойство из diff'а (например, PasswordHash, refresh-токены).
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class NotAuditedAttribute : Attribute
{
}

/// <summary>
/// PII-маркер: значения свойства в diff'е заменяются на "***".
/// Сам факт изменения фиксируется, но значения не утекают в audit-лог/Kafka.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class SensitiveAttribute : Attribute
{
}

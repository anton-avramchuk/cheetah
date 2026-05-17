using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Cheetah.Audit;

/// <summary>
/// Строит JSON-diff из EF EntityEntry. Уважает [NotAudited] и [Sensitive].
/// Формат: { "PropertyName": { "old": ..., "new": ... }, ... }
/// </summary>
public static class AuditChangesBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Build(EntityEntry entry, AuditAction action)
    {
        var changes = new Dictionary<string, object?>();
        var entityType = entry.Entity.GetType();

        foreach (var prop in entry.Properties)
        {
            var clrProp = entityType.GetProperty(prop.Metadata.Name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (clrProp is null) continue;
            if (clrProp.GetCustomAttribute<NotAuditedAttribute>() is not null) continue;

            var sensitive = clrProp.GetCustomAttribute<SensitiveAttribute>() is not null;

            switch (action)
            {
                case AuditAction.Created:
                    if (prop.CurrentValue is null) continue;
                    changes[prop.Metadata.Name] = new
                    {
                        old = (object?)null,
                        @new = sensitive ? "***" : prop.CurrentValue
                    };
                    break;

                case AuditAction.Deleted:
                    if (prop.OriginalValue is null) continue;
                    changes[prop.Metadata.Name] = new
                    {
                        old = sensitive ? "***" : prop.OriginalValue,
                        @new = (object?)null
                    };
                    break;

                case AuditAction.Updated:
                    if (!prop.IsModified) continue;
                    if (Equals(prop.OriginalValue, prop.CurrentValue)) continue;
                    changes[prop.Metadata.Name] = new
                    {
                        old = sensitive ? "***" : prop.OriginalValue,
                        @new = sensitive ? "***" : prop.CurrentValue
                    };
                    break;
            }
        }

        return JsonSerializer.Serialize(changes, JsonOptions);
    }
}

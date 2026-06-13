using System.Reflection;

namespace Cheetah.Core.Dapper.Mapping;

/// <summary>
/// Default conventions used when an entity is not explicitly mapped:
/// table name = type name (naively pluralized), columns = scalar property names.
/// </summary>
public static class MappingConventions
{
    /// <summary>
    /// Returns the scalar, materializable properties of an entity type.
    /// Navigation/collection properties and read-only/computed members are excluded,
    /// which also naturally skips <c>DomainEvents</c>.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetMappableProperties(Type entityType)
        => entityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead
                        && p.GetIndexParameters().Length == 0
                        && p.SetMethod is not null
                        && IsScalar(p.PropertyType));

    /// <summary>Naive table name convention: type name with a trailing <c>s</c>.</summary>
    public static string GetTableName(Type entityType)
    {
        var name = entityType.Name;
        return name.EndsWith('s') ? name : name + "s";
    }

    /// <summary>Determines whether a CLR type maps to a single SQL column.</summary>
    public static bool IsScalar(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;

        return t.IsPrimitive
               || t.IsEnum
               || t == typeof(string)
               || t == typeof(decimal)
               || t == typeof(Guid)
               || t == typeof(DateTime)
               || t == typeof(DateTimeOffset)
               || t == typeof(DateOnly)
               || t == typeof(TimeOnly)
               || t == typeof(TimeSpan)
               || t == typeof(byte[]);
    }
}

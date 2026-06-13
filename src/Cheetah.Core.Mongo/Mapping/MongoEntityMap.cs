using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace Cheetah.Core.Mongo.Mapping;

/// <summary>
/// Fluent base class for declaring how an entity maps to a MongoDB collection — the Mongo
/// counterpart of EF Core's <c>IEntityTypeConfiguration&lt;T&gt;</c>. Derive, configure in the
/// constructor, register as <c>[Export(Singleton, typeof(IMongoEntityMap))]</c>, and the registry
/// applies it on startup.
/// </summary>
/// <typeparam name="TEntity">The entity type being mapped.</typeparam>
/// <example>
/// <code>
/// [Export(LifetimeType.Singleton, typeof(IMongoEntityMap))]
/// public sealed class CustomerMap : MongoEntityMap&lt;Customer&gt;
/// {
///     public CustomerMap()
///     {
///         ToCollection("customers");
///         HasKey(x => x.Id);
///         Field(x => x.FullName, "full_name");
///         Ignore(x => x.TemporaryFlag);
///     }
/// }
/// </code>
/// </example>
public abstract class MongoEntityMap<TEntity> : IMongoEntityMap
    where TEntity : class
{
    private string? _collectionName;
    private PropertyInfo? _keyProperty;
    private readonly Dictionary<string, string> _elementNames = new();
    private readonly HashSet<string> _ignored = new();

    /// <inheritdoc />
    public Type EntityType => typeof(TEntity);

    /// <summary>Sets the collection name (defaults to the entity type name).</summary>
    protected void ToCollection(string collectionName) => _collectionName = collectionName;

    /// <summary>Declares the key property mapped to <c>_id</c> (defaults to <c>Id</c>).</summary>
    protected void HasKey<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        => _keyProperty = GetProperty(keySelector);

    /// <summary>Overrides the BSON element name for a property.</summary>
    protected void Field<TProp>(Expression<Func<TEntity, TProp>> selector, string elementName)
        => _elementNames[GetProperty(selector).Name] = elementName;

    /// <summary>Excludes a property from the document.</summary>
    protected void Ignore<TProp>(Expression<Func<TEntity, TProp>> selector)
        => _ignored.Add(GetProperty(selector).Name);

    /// <inheritdoc />
    public MongoEntityMapping BuildMapping()
    {
        var keyProperty = _keyProperty
            ?? typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException(
                $"Entity '{typeof(TEntity).Name}' has no 'Id' property; call HasKey(...) in its map.");

        return new MongoEntityMapping
        {
            EntityType = typeof(TEntity),
            CollectionName = _collectionName ?? MongoMappingConventions.GetCollectionName(typeof(TEntity)),
            KeyMemberName = keyProperty.Name,
        };
    }

    /// <inheritdoc />
    public BsonClassMap BuildClassMap()
    {
        var classMap = new BsonClassMap<TEntity>();
        classMap.AutoMap();

        foreach (var ignored in _ignored)
        {
            var member = classMap.GetMemberMap(ignored);
            if (member is not null)
                classMap.UnmapMember(member.MemberInfo);
        }

        foreach (var (property, elementName) in _elementNames)
            classMap.GetMemberMap(property)?.SetElementName(elementName);

        // Honour a custom key; the global EntityKeyConvention already designates "Id" otherwise.
        if (_keyProperty is not null && classMap.IdMemberMap?.MemberName != _keyProperty.Name)
        {
            var idMember = classMap.GetMemberMap(_keyProperty.Name);
            if (idMember is not null)
            {
                classMap.SetIdMember(idMember);
                idMember.SetElementName("_id");
            }
        }

        return classMap;
    }

    private static PropertyInfo GetProperty<TProp>(Expression<Func<TEntity, TProp>> selector)
    {
        var body = selector.Body as MemberExpression
                   ?? (selector.Body as UnaryExpression)?.Operand as MemberExpression;

        if (body?.Member is PropertyInfo property)
            return property;

        throw new ArgumentException($"Expression '{selector}' does not refer to a property.", nameof(selector));
    }
}

using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Cheetah.Core.Mongo.Mapping;

/// <summary>
/// Global, process-wide MongoDB serialization conventions for Cheetah entities. Registering these
/// before any (de)serialization is mandatory — most importantly it unmaps
/// <c>AggregateRoot.DomainEvents</c> from every aggregate, mirroring the EF Core rule
/// <c>builder.Ignore(e =&gt; e.DomainEvents)</c>; without it domain events would be persisted.
/// </summary>
public static class MongoMappingConventions
{
    private const string PackName = "Cheetah.Core.Mongo";
    private static readonly object Gate = new();
    private static bool _registered;

    /// <summary>The default collection name for an entity type when no explicit map overrides it.</summary>
    public static string GetCollectionName(Type entityType) => entityType.Name;

    /// <summary>
    /// Idempotently registers the Cheetah convention pack and the standard <see cref="Guid"/>
    /// serializer. Safe to call repeatedly and from multiple threads.
    /// </summary>
    public static void EnsureRegistered()
    {
        if (_registered)
            return;

        lock (Gate)
        {
            if (_registered)
                return;

            try
            {
                BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
                // Store DateTimeOffset as a BSON UTC date so range queries (e.g. NextAttemptAt) work.
                BsonSerializer.TryRegisterSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
            }
            catch (BsonSerializationException)
            {
                // A serializer was already registered by the host; honour it.
            }

            var pack = new ConventionPack
            {
                new IgnoreExtraElementsConvention(true),
                new MapWritablePropertiesConvention(),
                new IgnoreDomainEventsConvention(),
                new EntityKeyConvention(),
            };

            ConventionRegistry.Register(PackName, pack, _ => true);
            _registered = true;
        }
    }

    /// <summary>
    /// Maps every instance property that has a public getter and any setter (including
    /// <c>private</c>/<c>protected</c> set), so Cheetah domain entities — whose setters are
    /// non-public by design — are persisted. The driver's default member finder only maps public
    /// read-write properties, which would map nothing for these entities. Read-only computed
    /// properties (no setter, e.g. <c>DomainEvents</c>) are intentionally skipped.
    /// </summary>
    private sealed class MapWritablePropertiesConvention : ConventionBase, IClassMapConvention
    {
        public void Apply(BsonClassMap classMap)
        {
            var properties = classMap.ClassType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var property in properties)
            {
                if (property.GetMethod is not { IsPublic: true })
                    continue;
                if (property.SetMethod is null)
                    continue;
                if (property.GetIndexParameters().Length > 0)
                    continue;
                if (classMap.DeclaredMemberMaps.Any(m => m.MemberName == property.Name))
                    continue;

                classMap.MapMember(property);
            }
        }
    }

    /// <summary>
    /// Class-map convention that unmaps the <c>DomainEvents</c> member of any aggregate so domain
    /// events never reach the database.
    /// </summary>
    private sealed class IgnoreDomainEventsConvention : ConventionBase, IClassMapConvention
    {
        public void Apply(BsonClassMap classMap)
        {
            var member = classMap.DeclaredMemberMaps
                .FirstOrDefault(m => m.MemberName == "DomainEvents");

            if (member is not null)
                classMap.UnmapMember(member.MemberInfo);
        }
    }

    /// <summary>
    /// Designates the inherited <c>Entity&lt;TId&gt;.Id</c> member as the document <c>_id</c>. The
    /// driver's default id conventions do not pick up an inherited key with a non-public setter.
    /// </summary>
    private sealed class EntityKeyConvention : ConventionBase, IClassMapConvention
    {
        public void Apply(BsonClassMap classMap)
        {
            if (classMap.IdMemberMap is not null)
                return;

            var idMember = classMap.AllMemberMaps.FirstOrDefault(m => m.MemberName == "Id");
            if (idMember is not null)
            {
                classMap.SetIdMember(idMember);
                idMember.SetElementName("_id");
            }
        }
    }
}

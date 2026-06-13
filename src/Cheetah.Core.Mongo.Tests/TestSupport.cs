using Cheetah.Core.Domain;
using Cheetah.Core.Mongo.Mapping;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Cheetah.Core.Mongo.Tests;

/// <summary>A simple aggregate used to exercise filter translation and conventions.</summary>
public class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { }

    public static Product Create(string name, decimal price, int stock, bool isActive = true)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            Stock = stock,
            IsActive = isActive,
        };
}

/// <summary>An entity with an explicit map (collection, key, element rename, ignore).</summary>
public class Gadget : AggregateRoot<Guid>
{
    public string Title { get; private set; } = null!;
    public int Quantity { get; private set; }

    private Gadget() { }

    public static Gadget Create(string title, int quantity)
        => new() { Id = Guid.NewGuid(), Title = title, Quantity = quantity };
}

public sealed class GadgetMap : MongoEntityMap<Gadget>
{
    public GadgetMap()
    {
        ToCollection("gadgets");
        HasKey(x => x.Id);
        Field(x => x.Title, "title");
        Ignore(x => x.Quantity);
    }
}

/// <summary>An entity with no explicit map, used to verify convention fallback.</summary>
public class Widget : AggregateRoot<Guid>
{
    public string Label { get; private set; } = null!;

    private Widget() { }
}

internal static class FilterRenderer
{
    /// <summary>Renders a filter to a <see cref="BsonDocument"/> for assertions.</summary>
    public static BsonDocument Render<T>(FilterDefinition<T> filter)
    {
        MongoMappingConventions.EnsureRegistered();
        var serializer = BsonSerializer.SerializerRegistry.GetSerializer<T>();
        return filter.Render(new RenderArgs<T>(serializer, BsonSerializer.SerializerRegistry));
    }
}

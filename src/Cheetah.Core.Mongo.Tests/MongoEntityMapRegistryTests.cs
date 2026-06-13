using Cheetah.Core.Mongo.Mapping;
using Shouldly;

namespace Cheetah.Core.Mongo.Tests;

public class MongoEntityMapRegistryTests
{
    [Fact]
    public void Uses_explicit_map_when_registered()
    {
        var registry = new MongoEntityMapRegistry(new IMongoEntityMap[] { new GadgetMap() });

        var mapping = registry.GetMapping<Gadget>();

        mapping.CollectionName.ShouldBe("gadgets");
        mapping.KeyMemberName.ShouldBe("Id");
    }

    [Fact]
    public void Falls_back_to_convention_when_no_explicit_map()
    {
        var registry = new MongoEntityMapRegistry(Array.Empty<IMongoEntityMap>());

        var mapping = registry.GetMapping<Widget>();

        mapping.CollectionName.ShouldBe("Widget");
        mapping.KeyMemberName.ShouldBe("Id");
        mapping.EntityType.ShouldBe(typeof(Widget));
    }

    [Fact]
    public void Caches_mapping_per_type()
    {
        var registry = new MongoEntityMapRegistry(new IMongoEntityMap[] { new GadgetMap() });

        registry.GetMapping<Gadget>().ShouldBeSameAs(registry.GetMapping<Gadget>());
    }
}

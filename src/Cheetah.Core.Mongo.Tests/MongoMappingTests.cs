using Cheetah.Core.Mongo.Mapping;
using MongoDB.Bson.Serialization;
using Shouldly;

namespace Cheetah.Core.Mongo.Tests;

public class MongoMappingTests
{
    private static BsonClassMap<T> AutoMapped<T>()
    {
        MongoMappingConventions.EnsureRegistered();
        var classMap = new BsonClassMap<T>();
        classMap.AutoMap();
        classMap.Freeze(); // AllMemberMaps/IdMemberMap are only fully resolved after freezing.
        return classMap;
    }

    [Fact]
    public void Convention_unmaps_domain_events()
    {
        var classMap = AutoMapped<Product>();

        classMap.AllMemberMaps.ShouldNotContain(m => m.MemberName == "DomainEvents");
    }

    [Fact]
    public void Convention_maps_non_public_setter_properties()
    {
        var classMap = AutoMapped<Product>();

        var mapped = classMap.AllMemberMaps.Select(m => m.MemberName).ToList();
        mapped.ShouldContain(nameof(Product.Name));
        mapped.ShouldContain(nameof(Product.Price));
        mapped.ShouldContain(nameof(Product.IsActive));
    }

    [Fact]
    public void Convention_maps_id_to_underscore_id()
    {
        var classMap = AutoMapped<Product>();

        classMap.IdMemberMap.ShouldNotBeNull();
        classMap.IdMemberMap!.MemberName.ShouldBe("Id");
        classMap.IdMemberMap.ElementName.ShouldBe("_id");
    }

    [Fact]
    public void Explicit_map_builds_collection_and_key_metadata()
    {
        var mapping = new GadgetMap().BuildMapping();

        mapping.CollectionName.ShouldBe("gadgets");
        mapping.KeyMemberName.ShouldBe("Id");
        mapping.EntityType.ShouldBe(typeof(Gadget));
    }

    [Fact]
    public void Explicit_map_applies_element_rename_and_ignore()
    {
        var classMap = new GadgetMap().BuildClassMap();
        classMap.Freeze();

        classMap.GetMemberMap(nameof(Gadget.Title))!.ElementName.ShouldBe("title");
        classMap.AllMemberMaps.ShouldNotContain(m => m.MemberName == nameof(Gadget.Quantity));
        classMap.AllMemberMaps.ShouldNotContain(m => m.MemberName == "DomainEvents");
    }

    [Fact]
    public void Default_collection_name_is_entity_type_name()
    {
        MongoMappingConventions.GetCollectionName(typeof(Widget)).ShouldBe("Widget");
    }
}

using Cheetah.Core.Mongo.Specifications;
using Cheetah.Core.Specification;
using MongoDB.Driver;
using Shouldly;

namespace Cheetah.Core.Mongo.Tests;

public class MongoFilterBuilderTests
{
    private readonly MongoFilterBuilder _builder = new();

    private sealed class ProductByNameSpec(string name) : Specification<Product>
    {
        public override System.Linq.Expressions.Expression<Func<Product, bool>> ToExpression()
            => p => p.Name == name;
    }

    private sealed class ActiveAndPricedSpec(decimal minPrice) : Specification<Product>
    {
        public override System.Linq.Expressions.Expression<Func<Product, bool>> ToExpression()
            => p => p.IsActive && p.Price >= minPrice;
    }

    private sealed class NativePriceSpec(decimal min) : MongoSpecification<Product>
    {
        public override FilterDefinition<Product> ToFilter()
            => Builders<Product>.Filter.Gte(p => p.Price, min);
    }

    [Fact]
    public void Null_spec_builds_empty_filter()
    {
        var filter = _builder.Build<Product>(null);

        FilterRenderer.Render(filter).ShouldBe(new MongoDB.Bson.BsonDocument());
    }

    [Fact]
    public void Equality_expression_translates_to_field_match()
    {
        var filter = _builder.Build(new ProductByNameSpec("Widget"));

        var rendered = FilterRenderer.Render(filter);
        rendered["Name"].AsString.ShouldBe("Widget");
    }

    [Fact]
    public void Conjunction_translates_both_operands()
    {
        var filter = _builder.Build(new ActiveAndPricedSpec(10m));

        var rendered = FilterRenderer.Render(filter);
        rendered.ToString().ShouldContain("IsActive");
        rendered.ToString().ShouldContain("Price");
        rendered.ToString().ShouldContain("$gte");
    }

    [Fact]
    public void Native_specification_takes_fast_path()
    {
        var filter = _builder.Build(new NativePriceSpec(5m));

        var rendered = FilterRenderer.Render(filter);
        rendered["Price"]["$gte"].ToDecimal().ShouldBe(5m);
    }

    [Fact]
    public void Native_specification_to_expression_throws()
    {
        Should.Throw<NotSupportedException>(() => new NativePriceSpec(1m).ToExpression());
    }
}

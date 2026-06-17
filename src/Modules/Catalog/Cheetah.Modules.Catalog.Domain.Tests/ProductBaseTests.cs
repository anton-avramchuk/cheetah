using Cheetah.Modules.Catalog.DomainEvents;
using Cheetah.Modules.Catalog.Shared;
using Shouldly;

namespace Cheetah.Modules.Catalog.Domain.Tests;

public class ProductBaseTests
{
    [Fact]
    public void Create_sets_fields_and_raises_created_event()
    {
        var product = TestProduct.Create("SKU-1", "Widget", ProductType.Goods, UnitOfMeasure.Piece, brand: "Acme");

        product.Sku.ShouldBe("SKU-1");
        product.Name.ShouldBe("Widget");
        product.IsActive.ShouldBeTrue();
        product.Brand.ShouldBe("Acme");
        product.DomainEvents.OfType<ProductCreatedIntegrationEvent>().ShouldHaveSingleItem()
            .Sku.ShouldBe("SKU-1");
    }

    [Theory]
    [InlineData("", "Name")]
    [InlineData("SKU", "")]
    public void Create_requires_sku_and_name(string sku, string name)
        => Should.Throw<ArgumentException>(() => TestProduct.Create(sku, name));

    [Fact]
    public void Deactivate_is_idempotent_and_raises_event_once()
    {
        var product = TestProduct.Create("SKU-1", "Widget");
        product.ClearDomainEvents();

        product.Deactivate();
        product.Deactivate();

        product.IsActive.ShouldBeFalse();
        product.DomainEvents.OfType<ProductDeactivatedIntegrationEvent>().Count().ShouldBe(1);
    }

    [Fact]
    public void Activate_after_deactivate_raises_event()
    {
        var product = TestProduct.Create("SKU-1", "Widget");
        product.Deactivate();
        product.ClearDomainEvents();

        product.Activate();

        product.IsActive.ShouldBeTrue();
        product.DomainEvents.OfType<ProductActivatedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Update_changes_fields_and_raises_event()
    {
        var product = TestProduct.Create("SKU-1", "Widget");
        var categoryId = Guid.NewGuid();
        product.ClearDomainEvents();

        product.Update("Gadget", "new desc", categoryId);

        product.Name.ShouldBe("Gadget");
        product.Description.ShouldBe("new desc");
        product.CategoryId.ShouldBe(categoryId);
        product.DomainEvents.OfType<ProductUpdatedIntegrationEvent>().ShouldHaveSingleItem();
    }
}

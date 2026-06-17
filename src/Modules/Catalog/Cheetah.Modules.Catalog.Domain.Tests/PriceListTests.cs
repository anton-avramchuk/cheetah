using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.DomainEvents;
using Shouldly;

namespace Cheetah.Modules.Catalog.Domain.Tests;

public class PriceListTests
{
    [Fact]
    public void Create_normalizes_currency_to_upper()
    {
        var pl = PriceList.Create("Retail", "usd");
        pl.Currency.ShouldBe("USD");
    }

    [Fact]
    public void SetPrice_adds_item_and_raises_event()
    {
        var pl = PriceList.Create("Retail", "USD");
        var productId = Guid.NewGuid();

        pl.SetPrice(productId, 9.99m);

        pl.Items.ShouldHaveSingleItem().Price.ShouldBe(9.99m);
        pl.DomainEvents.OfType<PriceChangedIntegrationEvent>().ShouldHaveSingleItem().Price.ShouldBe(9.99m);
    }

    [Fact]
    public void SetPrice_same_product_and_tier_updates_existing_item()
    {
        var pl = PriceList.Create("Retail", "USD");
        var productId = Guid.NewGuid();

        pl.SetPrice(productId, 10m);
        pl.SetPrice(productId, 12m);

        pl.Items.ShouldHaveSingleItem().Price.ShouldBe(12m);
    }

    [Fact]
    public void ResolvePrice_picks_highest_matching_tier()
    {
        var pl = PriceList.Create("Retail", "USD");
        var productId = Guid.NewGuid();
        pl.SetPrice(productId, 10m);            // без порога
        pl.SetPrice(productId, 8m, minQty: 10); // от 10 шт
        pl.SetPrice(productId, 6m, minQty: 50); // от 50 шт

        pl.ResolvePrice(productId, 1).ShouldBe(10m);
        pl.ResolvePrice(productId, 10).ShouldBe(8m);
        pl.ResolvePrice(productId, 49).ShouldBe(8m);
        pl.ResolvePrice(productId, 100).ShouldBe(6m);
    }

    [Fact]
    public void ResolvePrice_returns_null_when_no_price_for_product()
    {
        var pl = PriceList.Create("Retail", "USD");
        pl.ResolvePrice(Guid.NewGuid(), 1).ShouldBeNull();
    }

    [Fact]
    public void SetPrice_rejects_negative_price()
        => Should.Throw<ArgumentOutOfRangeException>(() => PriceList.Create("R", "USD").SetPrice(Guid.NewGuid(), -1m));
}

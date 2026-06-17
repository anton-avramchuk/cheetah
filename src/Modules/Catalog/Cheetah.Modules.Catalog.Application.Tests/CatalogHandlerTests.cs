using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.Grid;
using Cheetah.Core.Specification;
using Cheetah.Modules.Catalog.Application.Categories;
using Cheetah.Modules.Catalog.Application.Exceptions;
using Cheetah.Modules.Catalog.Application.PriceLists;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.DomainEvents;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Catalog.Application.Tests;

public class CategoryHandlerTests
{
    private readonly Mock<IRepository<ProductCategory, Guid>> _repo = new();
    private readonly Mock<IGridRepository<ProductCategory, Guid>> _grid = new();

    [Fact]
    public async Task Create_root_category_adds_and_saves()
    {
        var handler = new CreateCategoryCommandHandler(_repo.Object);

        var id = await handler.HandleAsync(new CreateCategoryCommand("Electronics", null));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.Is<ProductCategory>(c => c.Name == "Electronics" && c.ParentId == null)), Times.Once);
    }

    [Fact]
    public async Task Create_child_with_missing_parent_throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);
        var handler = new CreateCategoryCommandHandler(_repo.Object);

        await Should.ThrowAsync<CatalogValidationException>(() =>
            handler.HandleAsync(new CreateCategoryCommand("Phones", Guid.NewGuid())).AsTask());
    }

    [Fact]
    public async Task Update_renames_and_reorders()
    {
        var category = ProductCategory.Create("Old", null);
        _repo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        var handler = new UpdateCategoryCommandHandler(_repo.Object);

        await handler.HandleAsync(new UpdateCategoryCommand(category.Id, "New", 5));

        category.Name.ShouldBe("New");
        category.Order.ShouldBe(5);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_missing_throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);
        var handler = new DeleteCategoryCommandHandler(_repo.Object);

        await Should.ThrowAsync<CatalogValidationException>(() =>
            handler.HandleAsync(new DeleteCategoryCommand(Guid.NewGuid())).AsTask());
    }

    [Fact]
    public async Task GetById_projects_via_grid_repository()
    {
        var id = Guid.NewGuid();
        _grid.Setup(r => r.GetByIdAsync<ProductCategoryDto>(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductCategoryDto { Id = id, Name = "C", Path = "/x" });
        var handler = new GetCategoryByIdQueryHandler(_grid.Object);

        var dto = await handler.HandleAsync(new GetCategoryByIdQuery(id));

        dto.ShouldNotBeNull();
        dto!.Id.ShouldBe(id);
    }

    [Fact]
    public async Task Grid_delegates_to_grid_repository()
    {
        _grid.Setup(r => r.GetGridAsync<ProductCategoryDto>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridResult<ProductCategoryDto>(new[] { new ProductCategoryDto { Id = Guid.NewGuid(), Name = "C", Path = "/x" } }, 1));
        var handler = new GetCategoriesGridQueryHandler(_grid.Object);

        var result = await handler.HandleAsync(new GetCategoriesGridQuery(1, 10, new List<SortDescriptor>(), null));

        result.Total.ShouldBe(1);
    }
}

public class PriceListHandlerTests
{
    private readonly Mock<IRepository<PriceList, Guid>> _repo = new();
    private readonly Mock<IGridRepository<PriceList, Guid>> _grid = new();
    private readonly Mock<IEventBus> _eventBus = new();

    [Fact]
    public async Task SetPrice_persists_and_publishes_price_changed()
    {
        var pl = PriceList.Create("Retail", "USD");
        _repo.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<PriceList>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pl);
        var handler = new SetPriceCommandHandler(_repo.Object, _eventBus.Object);
        var productId = Guid.NewGuid();

        await handler.HandleAsync(new SetPriceCommand(pl.Id, productId, 9.99m, null));

        pl.Items.ShouldHaveSingleItem().Price.ShouldBe(9.99m);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is PriceChangedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetPrice_missing_price_list_throws()
    {
        _repo.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<PriceList>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);
        var handler = new SetPriceCommandHandler(_repo.Object, _eventBus.Object);

        await Should.ThrowAsync<CatalogValidationException>(() =>
            handler.HandleAsync(new SetPriceCommand(Guid.NewGuid(), Guid.NewGuid(), 1m, null)).AsTask());
    }

    [Fact]
    public async Task Update_changes_header()
    {
        var pl = PriceList.Create("Old", "USD");
        _repo.Setup(r => r.GetByIdAsync(pl.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pl);
        var handler = new UpdatePriceListCommandHandler(_repo.Object);

        await handler.HandleAsync(new UpdatePriceListCommand(pl.Id, "New", true, null, null));

        pl.Name.ShouldBe("New");
        pl.IsDefault.ShouldBeTrue();
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_removes_price_list()
    {
        var pl = PriceList.Create("Retail", "USD");
        _repo.Setup(r => r.GetByIdAsync(pl.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pl);
        var handler = new DeletePriceListCommandHandler(_repo.Object);

        await handler.HandleAsync(new DeletePriceListCommand(pl.Id));

        _repo.Verify(r => r.Delete(pl), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolvePrice_uses_default_price_list_when_id_omitted()
    {
        var pl = PriceList.Create("Default", "EUR", isDefault: true);
        var productId = Guid.NewGuid();
        pl.SetPrice(productId, 5m);
        _repo.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<PriceList>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pl);
        var handler = new ResolvePriceQueryHandler(_repo.Object);

        var result = await handler.HandleAsync(new ResolvePriceQuery(null, productId, 1));

        result.ShouldNotBeNull();
        result!.Price.ShouldBe(5m);
        result.Currency.ShouldBe("EUR");
    }

    [Fact]
    public async Task GetById_maps_items()
    {
        var pl = PriceList.Create("Retail", "USD");
        pl.SetPrice(Guid.NewGuid(), 3m, minQty: 10);
        _repo.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<PriceList>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pl);
        var handler = new GetPriceListByIdQueryHandler(_repo.Object);

        var dto = await handler.HandleAsync(new GetPriceListByIdQuery(pl.Id));

        dto.ShouldNotBeNull();
        dto!.Items.ShouldHaveSingleItem().MinQty.ShouldBe(10);
    }

    [Fact]
    public async Task Grid_delegates_to_grid_repository()
    {
        _grid.Setup(r => r.GetGridAsync<PriceListGridViewModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridResult<PriceListGridViewModel>(
                new[] { new PriceListGridViewModel { Id = Guid.NewGuid(), Name = "R", Currency = "USD" } }, 1));
        var handler = new GetPriceListsGridQueryHandler(_grid.Object);

        var result = await handler.HandleAsync(new GetPriceListsGridQuery(1, 10, new List<SortDescriptor>(), null));

        result.Total.ShouldBe(1);
    }
}

using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.Grid;
using Cheetah.Core.Specification;
using Cheetah.Modules.Catalog.Application.Exceptions;
using Cheetah.Modules.Catalog.Application.Products;
using Cheetah.Modules.Catalog.DomainEvents;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Catalog.Application.Tests;

public class ProductHandlerTests
{
    private readonly Mock<IRepository<TestProduct, Guid>> _repo = new();
    private readonly Mock<IEventBus> _eventBus = new();

    [Fact]
    public async Task Create_AddsSavesAndPublishesCreatedEvent()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TestProduct>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new CreateProductCommandHandler<TestProduct, TestCreateRequest>(
            new TestProductFactory(), _repo.Object, _eventBus.Object);

        var id = await handler.HandleAsync(new CreateProductCommand<TestCreateRequest>(TestData.CreateRequest()));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.Is<TestProduct>(p => p.Sku == "SKU-1")), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is ProductCreatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_PassesExtensionField()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TestProduct>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = new CreateProductCommandHandler<TestProduct, TestCreateRequest>(
            new TestProductFactory(), _repo.Object, _eventBus.Object);

        var req = TestData.CreateRequest() with { Brand = "Acme" };
        await handler.HandleAsync(new CreateProductCommand<TestCreateRequest>(req));

        _repo.Verify(r => r.Add(It.Is<TestProduct>(p => p.Brand == "Acme")), Times.Once);
    }

    [Fact]
    public async Task Create_DuplicateSku_Throws()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TestProduct>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateProductCommandHandler<TestProduct, TestCreateRequest>(
            new TestProductFactory(), _repo.Object, _eventBus.Object);

        await Should.ThrowAsync<CatalogValidationException>(() =>
            handler.HandleAsync(new CreateProductCommand<TestCreateRequest>(TestData.CreateRequest())).AsTask());
        _repo.Verify(r => r.Add(It.IsAny<TestProduct>()), Times.Never);
    }

    [Fact]
    public async Task Update_NotFound_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestProduct?)null);
        var handler = new UpdateProductCommandHandler<TestProduct, TestUpdateRequest>(_repo.Object, _eventBus.Object);

        await Should.ThrowAsync<CatalogValidationException>(() =>
            handler.HandleAsync(new UpdateProductCommand<TestUpdateRequest>(
                Guid.NewGuid(), new TestUpdateRequest { Name = "x" })).AsTask());
    }

    [Fact]
    public async Task Update_ChangesFields_SavesAndPublishes()
    {
        var product = TestData.NewProduct();
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var handler = new UpdateProductCommandHandler<TestProduct, TestUpdateRequest>(_repo.Object, _eventBus.Object);

        await handler.HandleAsync(new UpdateProductCommand<TestUpdateRequest>(
            product.Id, new TestUpdateRequest { Name = "Renamed" }));

        product.Name.ShouldBe("Renamed");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is ProductUpdatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deactivate_PublishesDeactivatedEvent()
    {
        var product = TestData.NewProduct();
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var handler = new DeactivateProductCommandHandler<TestProduct>(_repo.Object, _eventBus.Object);

        await handler.HandleAsync(new DeactivateProductCommand(product.Id));

        product.IsActive.ShouldBeFalse();
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is ProductDeactivatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsProjectedDto()
    {
        var product = TestData.NewProduct();
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var handler = new GetProductByIdQueryHandler<TestProduct, TestProductDto>(_repo.Object, new TestProductProjector());

        var dto = await handler.HandleAsync(new GetProductByIdQuery<TestProductDto>(product.Id));

        dto.ShouldNotBeNull();
        dto!.Sku.ShouldBe("SKU-1");
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestProduct?)null);
        var handler = new GetProductByIdQueryHandler<TestProduct, TestProductDto>(_repo.Object, new TestProductProjector());

        (await handler.HandleAsync(new GetProductByIdQuery<TestProductDto>(Guid.NewGuid()))).ShouldBeNull();
    }

    [Fact]
    public async Task List_ReturnsProjectedItems()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TestProduct>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestProduct> { TestData.NewProduct(), TestData.NewProduct() });
        var handler = new ListProductsQueryHandler<TestProduct, TestProductDto>(_repo.Object, new TestProductProjector());

        var items = await handler.HandleAsync(new ListProductsQuery<TestProductDto>(null, null, true));

        items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Delete_RemovesProduct()
    {
        var product = TestData.NewProduct();
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        var handler = new DeleteProductCommandHandler<TestProduct>(_repo.Object);

        await handler.HandleAsync(new DeleteProductCommand(product.Id));

        _repo.Verify(r => r.Delete(product), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Grid_DelegatesToGridRepository()
    {
        var grid = new Mock<IGridRepository<TestProduct, Guid>>();
        grid.Setup(r => r.GetGridAsync<TestProductGridViewModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GridResult<TestProductGridViewModel>(
                new[] { new TestProductGridViewModel { Id = Guid.NewGuid(), Sku = "S", Name = "N" } }, 1));
        var handler = new GetProductsGridQueryHandler<TestProduct, TestProductGridViewModel>(grid.Object);

        var result = await handler.HandleAsync(
            new GetProductsGridQuery<TestProductGridViewModel>(1, 10, new List<SortDescriptor>(), null));

        result.Total.ShouldBe(1);
    }
}

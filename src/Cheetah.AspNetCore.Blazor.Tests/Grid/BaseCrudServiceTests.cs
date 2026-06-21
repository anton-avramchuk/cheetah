using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.Grid;
using Moq;

namespace Cheetah.AspNetCore.Blazor.Tests.Grid;

public class BaseCrudServiceTests
{
    private sealed class TestCrudService(IGridRepository<FakeEntity> repo)
        : BaseCrudService<FakeEntity, FakeGridViewModel, FakeDetailsViewModel, FakeCreateViewModel>(repo)
    {
        public override Task CreateAsync(FakeCreateViewModel model, CancellationToken ct = default)
            => Task.CompletedTask;

        public override Task UpdateAsync(Guid id, FakeDetailsViewModel model, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    [Fact]
    public async Task GetGridAsync_MapsPaging_AndProjectsResult()
    {
        var repo = new Mock<IGridRepository<FakeEntity>>();
        GridRequest? captured = null;
        repo.Setup(r => r.GetGridAsync<FakeGridViewModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GridRequest, CancellationToken>((g, _) => captured = g)
            .ReturnsAsync(new GridResult<FakeGridViewModel>(
                [new FakeGridViewModel { Id = Guid.NewGuid(), Name = "a" }], total: 42));

        var sut = new TestCrudService(repo.Object);

        var result = await sut.GetGridAsync(new CrmPageRequest { Page = 3, PageSize = 15 });

        captured.ShouldNotBeNull();
        captured!.Page.ShouldBe(3);
        captured.PageSize.ShouldBe(15);
        result.Total.ShouldBe(42);
        result.Data.Count().ShouldBe(1);
    }

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IGridRepository<FakeEntity>>();
        repo.Setup(r => r.GetByIdAsync<FakeDetailsViewModel>(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FakeDetailsViewModel { Name = "z" });

        var sut = new TestCrudService(repo.Object);

        var details = await sut.GetByIdAsync(id);

        details.ShouldNotBeNull();
        details!.Name.ShouldBe("z");
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityExists_DeletesAndSaves()
    {
        var id = Guid.NewGuid();
        var entity = new FakeEntity(id);
        var repo = new Mock<IGridRepository<FakeEntity>>();
        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var sut = new TestCrudService(repo.Object);

        await sut.DeleteAsync(id);

        repo.Verify(r => r.Delete(entity), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityMissing_DoesNothing()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IGridRepository<FakeEntity>>();
        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FakeEntity?)null);

        var sut = new TestCrudService(repo.Object);

        await sut.DeleteAsync(id);

        repo.Verify(r => r.Delete(It.IsAny<FakeEntity>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

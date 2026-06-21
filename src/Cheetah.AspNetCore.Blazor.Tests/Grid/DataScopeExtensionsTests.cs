using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.Grid;
using Moq;

namespace Cheetah.AspNetCore.Blazor.Tests.Grid;

public class DataScopeExtensionsTests
{
    private static (Mock<IGridRepository<FakeEntity>> Repo, Func<GridRequest?> Captured) RepoCapturing()
    {
        var repo = new Mock<IGridRepository<FakeEntity>>();
        GridRequest? captured = null;
        repo.Setup(r => r.GetGridAsync<FakeGridViewModel>(It.IsAny<GridRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GridRequest, CancellationToken>((g, _) => captured = g)
            .ReturnsAsync(new GridResult<FakeGridViewModel>([], 0));
        return (repo, () => captured);
    }

    [Fact]
    public async Task CanViewAll_DoesNotApplyOwnerFilter()
    {
        var (repo, captured) = RepoCapturing();
        var scope = new DataScope(Guid.NewGuid(), CanViewAll: true);

        await repo.Object.GetScopedGridAsync<FakeEntity, FakeGridViewModel>(
            new CrmPageRequest(), scope, CancellationToken.None, "OwnerId");

        captured()!.Filter.ShouldBeNull();
    }

    [Fact]
    public async Task RestrictedScope_SingleOwnerField_AppliesLeafEqualsFilter()
    {
        var (repo, captured) = RepoCapturing();
        var userId = Guid.NewGuid();
        var scope = new DataScope(userId, CanViewAll: false);

        await repo.Object.GetScopedGridAsync<FakeEntity, FakeGridViewModel>(
            new CrmPageRequest(), scope, CancellationToken.None, "OwnerId");

        var filter = captured()!.Filter;
        filter.ShouldNotBeNull();
        filter!.Field.ShouldBe("OwnerId");
        filter.Operator.ShouldBe("eq");
        filter.Value.ShouldBe(userId);
        filter.Filters.ShouldBeEmpty();
    }

    [Fact]
    public async Task RestrictedScope_MultipleOwnerFields_CombinesWithOr()
    {
        var (repo, captured) = RepoCapturing();
        var userId = Guid.NewGuid();
        var scope = new DataScope(userId, CanViewAll: false);

        await repo.Object.GetScopedGridAsync<FakeEntity, FakeGridViewModel>(
            new CrmPageRequest(), scope, CancellationToken.None, "OwnerId", "ManagerId");

        var filter = captured()!.Filter;
        filter.ShouldNotBeNull();
        filter!.Logic.ShouldBe("or");
        filter.Filters.Count.ShouldBe(2);
        filter.Filters.Select(f => f.Field).ShouldBe(["OwnerId", "ManagerId"]);
        filter.Filters.ShouldAllBe(f => (Guid)f.Value! == userId && f.Operator == "eq");
    }

    [Fact]
    public async Task RestrictedScope_WithoutOwnerFields_AppliesNoFilter()
    {
        var (repo, captured) = RepoCapturing();
        var scope = new DataScope(Guid.NewGuid(), CanViewAll: false);

        await repo.Object.GetScopedGridAsync<FakeEntity, FakeGridViewModel>(
            new CrmPageRequest(), scope, CancellationToken.None);

        captured()!.Filter.ShouldBeNull();
    }
}

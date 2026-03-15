using Cheetah.BackgroundTasks;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Domain;
using Cheetah.Core.Specification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cheetah.BackgroundTasks.Tests;

public class SyncBackgroundTaskTests
{
    // ── Fakes ────────────────────────────────────────────────────────────────

    private record FakeDto(Guid Id, string Name);

    private class FakeEntity : Entity<Guid>
    {
        public string Name { get; private set; } = string.Empty;
        public string ContentHash { get; private set; } = string.Empty;

        private FakeEntity() { }

        public static FakeEntity CreateNew(Guid id, string name) =>
            new() { Id = id, Name = name, ContentHash = name };

        public void Update(string name)
        {
            Name = name;
            ContentHash = name;
        }
    }

    private class FakeRepository : IRepository<FakeEntity, Guid>
    {
        private readonly List<FakeEntity> _store = [];

        public IReadOnlyList<FakeEntity> Store => _store;
        public int SaveChangesCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }

        public void Add(FakeEntity entity) => _store.Add(entity);
        public void Update(FakeEntity entity) => UpdateCallCount++;
        public void Delete(FakeEntity entity) => _store.Remove(entity);

        public ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return ValueTask.FromResult(0);
        }

        public ValueTask<FakeEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.FirstOrDefault(x => x.Id == id));

        public ValueTask<FakeEntity?> GetBySpecAsync(ISpecification<FakeEntity> spec, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.FirstOrDefault(spec.ToExpression().Compile()));

        public ValueTask<List<FakeEntity>> GetAllAsync(ISpecification<FakeEntity>? spec = null, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.ToList());

        public ValueTask<bool> ExistsAsync(ISpecification<FakeEntity> spec, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.Any(spec.ToExpression().Compile()));

        public IQueryable<FakeEntity> AsQueryable() => _store.AsQueryable();
        public IQueryable<FakeEntity> AsNoTrackingQueryable() => _store.AsQueryable();
    }

    private class FakeRemoteService
    {
        private List<FakeDto> _items = [];

        public void SetItems(params FakeDto[] items) => _items = [..items];

        public ValueTask<IReadOnlyList<FakeDto>> GetAllAsync(CancellationToken ct)
            => ValueTask.FromResult<IReadOnlyList<FakeDto>>(_items);
    }

    private class TestSyncTask(IServiceScopeFactory scopeFactory)
        : SyncBackgroundTask<FakeRemoteService, FakeDto, FakeEntity, Guid>(
            scopeFactory, NullLogger.Instance)
    {
        public override TimeSpan Period => TimeSpan.FromMinutes(1);

        protected override async ValueTask<IReadOnlyList<FakeDto>> FetchAsync(
            FakeRemoteService service, CancellationToken ct)
            => await service.GetAllAsync(ct);

        protected override Guid GetId(FakeDto vm) => vm.Id;
        protected override string ComputeItemHash(FakeDto vm) => vm.Name; // hash == name for tests
        protected override string GetEntityContentHash(FakeEntity entity) => entity.ContentHash;
        protected override FakeEntity Create(FakeDto vm) => FakeEntity.CreateNew(vm.Id, vm.Name);
        protected override void Update(FakeEntity entity, FakeDto vm) => entity.Update(vm.Name);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (TestSyncTask task, FakeRemoteService service, FakeRepository repository)
        CreateFixture()
    {
        var service = new FakeRemoteService();
        var repository = new FakeRepository();

        var services = new ServiceCollection();
        services.AddSingleton(service);
        services.AddSingleton<IRepository<FakeEntity, Guid>>(repository);
        var sp = services.BuildServiceProvider();

        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
        var task = new TestSyncTask(scopeFactory);

        return (task, service, repository);
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task WhenNoItemsReturned_SkipsSyncEntirely()
    {
        var (task, service, repository) = CreateFixture();
        service.SetItems(); // empty

        await task.ExecuteAsync(CancellationToken.None);

        repository.Store.ShouldBeEmpty();
        repository.SaveChangesCallCount.ShouldBe(0);
    }

    [Fact]
    public async Task WhenNewItems_AddsThemAndSavesChanges()
    {
        var (task, service, repository) = CreateFixture();
        var id = Guid.NewGuid();
        service.SetItems(new FakeDto(id, "Alpha"));

        await task.ExecuteAsync(CancellationToken.None);

        repository.Store.ShouldHaveSingleItem();
        repository.Store[0].Id.ShouldBe(id);
        repository.Store[0].Name.ShouldBe("Alpha");
        repository.SaveChangesCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task WhenContentHashUnchanged_SkipsUpdate()
    {
        var (task, service, repository) = CreateFixture();
        var id = Guid.NewGuid();
        service.SetItems(new FakeDto(id, "Alpha"));

        await task.ExecuteAsync(CancellationToken.None); // first run: adds entity
        await task.ExecuteAsync(CancellationToken.None); // second run: batch hash differs (new run), entity hash matches

        // entity was added once, never updated
        repository.UpdateCallCount.ShouldBe(0);
        repository.SaveChangesCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task WhenContentHashChanged_UpdatesEntity()
    {
        var (task, service, repository) = CreateFixture();
        var id = Guid.NewGuid();
        service.SetItems(new FakeDto(id, "Alpha"));
        await task.ExecuteAsync(CancellationToken.None);

        service.SetItems(new FakeDto(id, "Alpha Updated"));
        await task.ExecuteAsync(CancellationToken.None);

        repository.UpdateCallCount.ShouldBe(1);
        repository.Store[0].Name.ShouldBe("Alpha Updated");
        repository.SaveChangesCallCount.ShouldBe(2);
    }

    [Fact]
    public async Task WhenBatchHashUnchanged_SkipsDatabaseEntirely()
    {
        var (task, service, repository) = CreateFixture();
        service.SetItems(new FakeDto(Guid.NewGuid(), "Alpha"));

        await task.ExecuteAsync(CancellationToken.None); // first run
        var saveCountAfterFirst = repository.SaveChangesCallCount;

        await task.ExecuteAsync(CancellationToken.None); // same data → batch hash matches

        repository.SaveChangesCallCount.ShouldBe(saveCountAfterFirst); // no extra DB calls
    }

    [Fact]
    public async Task WhenMixedChanges_ProcessesOnlyChangedItems()
    {
        var (task, service, repository) = CreateFixture();
        var unchangedId = Guid.NewGuid();
        var changedId = Guid.NewGuid();
        var newId = Guid.NewGuid();

        service.SetItems(
            new FakeDto(unchangedId, "Unchanged"),
            new FakeDto(changedId, "Original"));
        await task.ExecuteAsync(CancellationToken.None);

        service.SetItems(
            new FakeDto(unchangedId, "Unchanged"),   // no change
            new FakeDto(changedId, "Modified"),      // changed
            new FakeDto(newId, "New"));              // new

        await task.ExecuteAsync(CancellationToken.None);

        repository.Store.Count.ShouldBe(3);
        repository.UpdateCallCount.ShouldBe(1);     // only changedId
        repository.SaveChangesCallCount.ShouldBe(2);
    }

    [Fact]
    public async Task WhenAllHashesMatch_SaveChangesNotCalled()
    {
        var (task, service, repository) = CreateFixture();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        service.SetItems(new FakeDto(id1, "A"), new FakeDto(id2, "B"));
        await task.ExecuteAsync(CancellationToken.None); // seeds

        // Change ordering (different batch hash) but same content
        service.SetItems(new FakeDto(id2, "B"), new FakeDto(id1, "A"));
        await task.ExecuteAsync(CancellationToken.None);

        repository.SaveChangesCallCount.ShouldBe(1); // only the initial seed
        repository.UpdateCallCount.ShouldBe(0);
    }
}

using Cheetah.Core.Events;
using Cheetah.Core.Specification;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Application.Abstractions;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Domain.Repositories;

namespace Cheetah.Modules.FeatureManagement.Application.Tests;

internal sealed class TestFlag : FeatureFlagBase
{
    private TestFlag() { }

    public static TestFlag Create(string key, string name, string ownerService,
        FeatureValueType valueType = FeatureValueType.Bool, string? description = null)
    {
        var flag = new TestFlag();
        flag.InitializeCore(Guid.NewGuid(), key, name, ownerService, valueType, description);
        return flag;
    }
}

internal sealed record TestCreateRequest : CreateFeatureFlagRequestBase;

internal sealed record TestDto : FeatureFlagDtoBase;

internal sealed class TestFactory : IFeatureFlagFactory<TestFlag, TestCreateRequest>
{
    public TestFlag Create(TestCreateRequest request)
        => TestFlag.Create(request.Key, request.Name, request.OwnerService, request.ValueType, request.Description);

    public TestFlag CreateFromDescriptor(FeatureDefinitionDescriptor d)
    {
        var flag = TestFlag.Create(d.Key, d.Name, d.OwnerService, d.ValueType, d.Description);
        if (d.ParentKey is not null) flag.SetParent(d.ParentKey);
        return flag;
    }
}

internal sealed class TestProjector : IFeatureFlagProjector<TestFlag, TestDto>
{
    public TestDto ToDto(TestFlag flag) => new()
    {
        Id = flag.Id,
        Key = flag.Key,
        Name = flag.Name,
        OwnerService = flag.OwnerService,
        Enabled = flag.Enabled,
        ValueType = flag.ValueType,
        IsActive = flag.IsActive,
        Rules = flag.Rules.Select(r => new TargetingRuleDto(r.Order, r.FilterName,
            new Dictionary<string, object?>(), r.ResultVariant, r.Negate)).ToArray()
    };
}

/// <summary>In-memory репозиторий флага для тестов хендлеров.</summary>
internal sealed class InMemoryFlagRepository : IFeatureFlagRepository<TestFlag>
{
    private readonly List<TestFlag> _store = new();
    public int SaveCount { get; private set; }

    public InMemoryFlagRepository(params TestFlag[] seed) => _store.AddRange(seed);

    public ValueTask<TestFlag?> GetByKeyAsync(string key, bool includeChildren, CancellationToken ct = default)
        => ValueTask.FromResult(_store.FirstOrDefault(f => f.Key == key));

    public ValueTask<IReadOnlyList<TestFlag>> ListAsync(ISpecification<TestFlag>? spec, bool includeChildren, CancellationToken ct = default)
    {
        IEnumerable<TestFlag> q = _store;
        if (spec is not null) q = q.Where(spec.ToExpression().Compile());
        return ValueTask.FromResult<IReadOnlyList<TestFlag>>(q.ToArray());
    }

    public ValueTask<TestFlag?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => ValueTask.FromResult(_store.FirstOrDefault(f => f.Id == id));
    public ValueTask<TestFlag?> GetBySpecAsync(ISpecification<TestFlag> spec, CancellationToken ct = default)
        => ValueTask.FromResult(_store.FirstOrDefault(spec.ToExpression().Compile()));
    public ValueTask<List<TestFlag>> GetAllAsync(ISpecification<TestFlag>? spec = null, CancellationToken ct = default)
        => ValueTask.FromResult(_store.ToList());
    public ValueTask<bool> ExistsAsync(ISpecification<TestFlag> spec, CancellationToken ct = default)
        => ValueTask.FromResult(_store.Any(spec.ToExpression().Compile()));
    public IQueryable<TestFlag> AsQueryable() => _store.AsQueryable();
    public IQueryable<TestFlag> AsNoTrackingQueryable() => _store.AsQueryable();
    public void Add(TestFlag entity) => _store.Add(entity);
    public void Update(TestFlag entity) { }
    public void Delete(TestFlag entity) => _store.Remove(entity);
    public ValueTask<int> SaveChangesAsync(CancellationToken ct = default) { SaveCount++; return ValueTask.FromResult(0); }
}

/// <summary>Записывающая шина для проверки публикации событий.</summary>
internal sealed class RecordingEventBus : IEventBus
{
    public List<IEvent> Published { get; } = new();

    public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IEvent
    {
        Published.Add(@event);
        return ValueTask.CompletedTask;
    }

    public ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken ct = default) where TEvent : IEvent
    {
        Published.AddRange(events.Cast<IEvent>());
        return ValueTask.CompletedTask;
    }

    public void Subscribe<TEvent, THandler>() where TEvent : IEvent where THandler : IEventHandler<TEvent> { }
}

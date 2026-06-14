using Cheetah.Core.Specification;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;

namespace Cheetah.Modules.Calendar.Application.Tests;

/// <summary>In-memory репозиторий триггеров для проверки планировщика без EF.</summary>
internal sealed class FakeTriggerRepository : IReminderTriggerRepository
{
    public List<ReminderTrigger> Items { get; } = new();
    public int SaveCount { get; private set; }

    public void Add(ReminderTrigger entity) => Items.Add(entity);
    public void Update(ReminderTrigger entity) { }
    public void Delete(ReminderTrigger entity) => Items.Remove(entity);

    public ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return ValueTask.FromResult(Items.Count);
    }

    public ValueTask<List<ReminderTrigger>> GetAllAsync(
        ISpecification<ReminderTrigger>? spec = null, CancellationToken cancellationToken = default)
    {
        var q = spec is null ? Items : Items.Where(spec.ToExpression().Compile());
        return ValueTask.FromResult(q.ToList());
    }

    public ValueTask<List<ReminderTrigger>> GetDueAsync(DateTime nowUtc, int batchSize, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Items
            .Where(t => t.Status == Shared.ReminderTriggerStatus.Pending && t.FireAtUtc <= nowUtc)
            .OrderBy(t => t.FireAtUtc).Take(batchSize).ToList());

    public ValueTask<ReminderTrigger?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Items.FirstOrDefault(t => t.Id == id));

    public ValueTask<ReminderTrigger?> GetBySpecAsync(ISpecification<ReminderTrigger> spec, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Items.FirstOrDefault(spec.ToExpression().Compile()));

    public ValueTask<bool> ExistsAsync(ISpecification<ReminderTrigger> spec, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Items.Any(spec.ToExpression().Compile()));

    public IQueryable<ReminderTrigger> AsQueryable() => Items.AsQueryable();
    public IQueryable<ReminderTrigger> AsNoTrackingQueryable() => Items.AsQueryable();
}

/// <summary>Экспандер с заранее заданными экземплярами (для изоляции логики планировщика).</summary>
internal sealed class FakeExpander : IRecurrenceExpander
{
    private readonly IReadOnlyList<Occurrence> _occurrences;
    public FakeExpander(IReadOnlyList<Occurrence> occurrences) => _occurrences = occurrences;

    public IEnumerable<Occurrence> Expand(CalendarEvent @event, DateTime fromUtc, DateTime toUtc)
        => _occurrences.Where(o => o.StartUtc >= fromUtc && o.StartUtc < toUtc);
}

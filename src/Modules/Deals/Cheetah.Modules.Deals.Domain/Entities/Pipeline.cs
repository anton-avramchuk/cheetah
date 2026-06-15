using Cheetah.Core.Domain;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Domain.Entities;

/// <summary>
/// Воронка продаж — агрегат. Стадии (<see cref="PipelineStage"/>) живут внутри его границы
/// (child-entities) и всегда меняются вместе с воронкой в одной транзакции.
/// </summary>
public sealed class Pipeline : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<PipelineStage> _stages = new();

    public string Name { get; private set; } = null!;
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyList<PipelineStage> Stages => _stages;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private Pipeline() { } // EF

    public static Pipeline Create(string name, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Pipeline
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            IsDefault = isDefault,
            IsActive = true
        };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void MarkDefault() => IsDefault = true;
    public void ClearDefault() => IsDefault = false;

    public PipelineStage AddStage(string name, int order, int probability, StageType type)
    {
        var stage = PipelineStage.Create(Id, name, order, probability, type);
        _stages.Add(stage);
        return stage;
    }

    /// <summary>Первая открытая стадия по порядку — на неё попадает новая сделка.</summary>
    public PipelineStage FirstStage()
    {
        var first = _stages
            .Where(s => s.Type == StageType.Open)
            .OrderBy(s => s.Order)
            .FirstOrDefault();
        return first ?? throw new InvalidOperationException(
            $"Pipeline '{Name}' ({Id}) has no open stage to start a deal.");
    }

    public PipelineStage GetStage(Guid stageId)
        => _stages.FirstOrDefault(s => s.Id == stageId)
           ?? throw new InvalidOperationException($"Stage {stageId} does not belong to pipeline {Id}.");
}

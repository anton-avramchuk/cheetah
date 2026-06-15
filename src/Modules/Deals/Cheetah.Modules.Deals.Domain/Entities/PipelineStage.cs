using Cheetah.Core.Domain;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Domain.Entities;

/// <summary>Стадия воронки — child-entity внутри <see cref="Pipeline"/>.</summary>
public sealed class PipelineStage : Entity<Guid>
{
    public Guid PipelineId { get; private set; }
    public string Name { get; private set; } = null!;
    public int Order { get; private set; }

    /// <summary>Вероятность выигрыша на этой стадии, 0..100.</summary>
    public int Probability { get; private set; }

    public StageType Type { get; private set; }

    private PipelineStage() { } // EF

    internal static PipelineStage Create(Guid pipelineId, string name, int order, int probability, StageType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (probability is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be in [0, 100].");

        return new PipelineStage
        {
            Id = Guid.NewGuid(),
            PipelineId = pipelineId,
            Name = name.Trim(),
            Order = order,
            Probability = probability,
            Type = type
        };
    }
}

using Cheetah.Core.Domain;
using Cheetah.Modules.Workflow.Shared;

namespace Cheetah.Modules.Workflow.Domain.Entities;

/// <summary>Привязка правила к триггеру: имя интеграционного события (<see cref="TriggerType.Event"/>)
/// либо cron-выражение (<see cref="TriggerType.Schedule"/>).</summary>
public sealed class TriggerBinding : Entity<Guid>
{
    public Guid RuleId { get; private set; }
    public TriggerType TriggerType { get; private set; }
    public string TriggerKey { get; private set; } = null!;
    public string? Parameters { get; private set; }

    private TriggerBinding() { }

    public static TriggerBinding Event(Guid ruleId, string eventName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        return new TriggerBinding
        {
            Id = Guid.NewGuid(), RuleId = ruleId,
            TriggerType = TriggerType.Event, TriggerKey = eventName.Trim()
        };
    }

    public static TriggerBinding Schedule(Guid ruleId, string cron)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cron);
        return new TriggerBinding
        {
            Id = Guid.NewGuid(), RuleId = ruleId,
            TriggerType = TriggerType.Schedule, TriggerKey = cron.Trim()
        };
    }
}

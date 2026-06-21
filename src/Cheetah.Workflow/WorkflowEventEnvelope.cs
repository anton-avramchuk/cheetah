using Cheetah.Core.Events;

namespace Cheetah.Workflow;

/// <summary>
/// Нормализованный «конверт» любого интеграционного события для движка правил Workflow.
/// <para>
/// Forwarder (на стороне источников) дублирует каждое <see cref="EventBase"/> в firehose-канал как
/// конверт; движок Workflow подписан на ОДИН тип <see cref="WorkflowEventEnvelope"/> и не зависит от
/// <c>DomainEvents</c>-сборок отдельных модулей. <see cref="Payload"/> — плоский разбор полей события,
/// пригодный как контекст для JsonLogic-условий правил.
/// </para>
/// </summary>
/// <remarks>
/// Конверт сам является <see cref="EventBase"/> и едет по шине, поэтому имеет собственные
/// <see cref="EventBase.EventId"/>/<see cref="EventBase.OccurredAt"/>. <see cref="SourceEventId"/> — это
/// <c>EventId</c> ИСХОДНОГО события (используется для дедупа срабатываний правил).
/// </remarks>
public sealed record WorkflowEventEnvelope(
    string EventName,
    Guid SourceEventId,
    IReadOnlyDictionary<string, object?> Payload,
    string? SourceService = null,
    Guid? TenantId = null) : EventBase;

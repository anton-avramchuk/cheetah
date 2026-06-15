using Cheetah.Core.Events;

namespace Cheetah.Modules.Leads.DomainEvents;

/// <summary>Лид создан (захвачен из формы/импорта/рекламы).</summary>
public record LeadCreatedIntegrationEvent(Guid LeadId, string Source) : EventBase;

/// <summary>Лид квалифицирован.</summary>
public record LeadQualifiedIntegrationEvent(Guid LeadId) : EventBase;

/// <summary>Лид дисквалифицирован.</summary>
public record LeadDisqualifiedIntegrationEvent(Guid LeadId, string Reason) : EventBase;

/// <summary>Лид сконвертирован в клиента (+ опционально сделку).</summary>
public record LeadConvertedIntegrationEvent(Guid LeadId, Guid CustomerId, Guid? DealId) : EventBase;

using Cheetah.Core.Events;

namespace Cheetah.Features.Events;

/// <summary>
/// Event raised when a feature is disabled for a tenant
/// </summary>
public record FeatureDisabledEvent(
    Guid TenantId,
    string FeatureId,
    DateTime DisabledAt
) : EventBase;

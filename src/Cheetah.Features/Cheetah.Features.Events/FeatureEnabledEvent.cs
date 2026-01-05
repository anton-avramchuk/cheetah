using Cheetah.Core.Events;

namespace Cheetah.Features.Events;

/// <summary>
/// Event raised when a feature is enabled for a tenant
/// </summary>
public record FeatureEnabledEvent(
    Guid TenantId,
    string FeatureId,
    DateTime EnabledAt
) : EventBase;

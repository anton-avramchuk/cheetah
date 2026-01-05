using Cheetah.Core.Events;

namespace Cheetah.Features.Events;

/// <summary>
/// Event raised when a new feature is created
/// </summary>
public record FeatureCreatedEvent(
    string FeatureId,
    string Name,
    string DisplayName,
    bool IsEnabledByDefault,
    DateTime CreatedAt
) : EventBase;

using Cheetah.Core.Domain;
using Cheetah.Features.Events;

namespace Cheetah.Features.Domain.Entities;

/// <summary>
/// Feature flag definition
/// </summary>
public class Feature : AggregateRoot<string>
{
    public string Name { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsEnabledByDefault { get; private set; }
    public string? Group { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Feature()
    {
    }

    public static Feature Create(string id, string displayName, string? description = null, bool isEnabledByDefault = false, string? group = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Feature ID cannot be empty", nameof(id));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty", nameof(displayName));

        var feature = new Feature
        {
            Id = id,
            Name = id,
            DisplayName = displayName,
            Description = description,
            IsEnabledByDefault = isEnabledByDefault,
            Group = group,
            CreatedAt = DateTime.UtcNow
        };

        feature.AddDomainEvent(new FeatureCreatedEvent(
            id,
            id,
            displayName,
            isEnabledByDefault,
            feature.CreatedAt
        ));

        return feature;
    }

    public void Update(string displayName, string? description = null, string? group = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty", nameof(displayName));

        DisplayName = displayName;
        Description = description;
        Group = group;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefaultEnabled(bool isEnabled)
    {
        IsEnabledByDefault = isEnabled;
        UpdatedAt = DateTime.UtcNow;
    }
}

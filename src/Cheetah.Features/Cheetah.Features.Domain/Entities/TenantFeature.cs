using Cheetah.Core.Domain;
using Cheetah.Features.Events;

namespace Cheetah.Features.Domain.Entities;

/// <summary>
/// Feature flag state for a specific tenant
/// </summary>
public class TenantFeature : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public string FeatureId { get; private set; } = null!;
    public bool IsEnabled { get; private set; }
    public DateTime? EnabledAt { get; private set; }
    public DateTime? DisabledAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TenantFeature()
    {
    }

    public static TenantFeature Create(Guid tenantId, string featureId, bool isEnabled = true)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(featureId))
            throw new ArgumentException("Feature ID cannot be empty", nameof(featureId));

        var now = DateTime.UtcNow;
        var tenantFeature = new TenantFeature
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FeatureId = featureId,
            IsEnabled = isEnabled,
            EnabledAt = isEnabled ? now : null,
            DisabledAt = isEnabled ? null : now,
            CreatedAt = now,
            UpdatedAt = now
        };

        return tenantFeature;
    }

    public void Enable()
    {
        if (IsEnabled)
            return;

        IsEnabled = true;
        EnabledAt = DateTime.UtcNow;
        DisabledAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        if (!IsEnabled)
            return;

        IsEnabled = false;
        DisabledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

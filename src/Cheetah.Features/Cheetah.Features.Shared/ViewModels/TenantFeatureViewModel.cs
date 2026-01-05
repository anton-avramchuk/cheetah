namespace Cheetah.Features.Shared.ViewModels;

public class TenantFeatureViewModel
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FeatureId { get; set; } = null!;
    public string FeatureName { get; set; } = null!;
    public string FeatureDisplayName { get; set; } = null!;
    public string? FeatureDescription { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? EnabledAt { get; set; }
    public DateTime? DisabledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

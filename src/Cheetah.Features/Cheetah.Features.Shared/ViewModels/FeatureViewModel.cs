namespace Cheetah.Features.Shared.ViewModels;

public class FeatureViewModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsEnabledByDefault { get; set; }
    public string? Group { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

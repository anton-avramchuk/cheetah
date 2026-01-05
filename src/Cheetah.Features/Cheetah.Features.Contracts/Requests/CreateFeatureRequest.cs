using System.ComponentModel.DataAnnotations;

namespace Cheetah.Features.Contracts.Requests;

public class CreateFeatureRequest
{
    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Id { get; set; } = null!;

    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string DisplayName { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsEnabledByDefault { get; set; }

    [StringLength(128)]
    public string? Group { get; set; }
}

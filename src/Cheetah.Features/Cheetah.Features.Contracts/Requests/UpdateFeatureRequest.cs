using System.ComponentModel.DataAnnotations;

namespace Cheetah.Features.Contracts.Requests;

public class UpdateFeatureRequest
{
    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string DisplayName { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(128)]
    public string? Group { get; set; }
}

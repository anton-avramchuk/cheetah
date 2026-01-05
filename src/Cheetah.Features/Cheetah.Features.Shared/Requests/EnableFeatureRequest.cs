using System.ComponentModel.DataAnnotations;

namespace Cheetah.Features.Shared.Requests;

public class EnableFeatureRequest
{
    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string FeatureId { get; set; } = null!;
}

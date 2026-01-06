namespace Cheetah.Identity.Contracts.ViewModels;

/// <summary>
/// Detailed role information with permissions
/// </summary>
public class RoleDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Permissions (claims) assigned to this role
    /// </summary>
    public List<ClaimViewModel> Claims { get; set; } = [];

    /// <summary>
    /// All permission values (extracted from Claims)
    /// </summary>
    public List<string> Permissions { get; set; } = [];
}

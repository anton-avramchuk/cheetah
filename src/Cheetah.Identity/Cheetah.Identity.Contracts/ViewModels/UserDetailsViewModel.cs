namespace Cheetah.Identity.Contracts.ViewModels;

/// <summary>
/// Detailed user information with roles and claims
/// </summary>
public class UserDetailsViewModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Roles assigned to this user
    /// </summary>
    public List<RoleViewModel> Roles { get; set; } = [];

    /// <summary>
    /// Personal claims (permissions) assigned directly to user
    /// </summary>
    public List<ClaimViewModel> Claims { get; set; } = [];

    /// <summary>
    /// All permissions (from roles + personal claims)
    /// </summary>
    public List<string> Permissions { get; set; } = [];
}

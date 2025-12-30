namespace Cheetah.Identity.Shared.ViewModels;

/// <summary>
/// Basic user information view model
/// </summary>
public class UserViewModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}

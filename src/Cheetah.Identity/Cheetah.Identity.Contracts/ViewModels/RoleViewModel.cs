namespace Cheetah.Identity.Contracts.ViewModels;

/// <summary>
/// Basic role information view model
/// </summary>
public class RoleViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}

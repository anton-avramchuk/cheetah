namespace Cheetah.Identity.Contracts.Requests;

/// <summary>
/// Request to create a new role
/// </summary>
public class CreateRoleRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

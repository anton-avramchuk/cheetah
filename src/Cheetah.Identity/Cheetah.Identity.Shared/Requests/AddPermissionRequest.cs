namespace Cheetah.Identity.Shared.Requests;

/// <summary>
/// Request to add a permission (claim) to a role or user
/// </summary>
public class AddPermissionRequest
{
    public string Permission { get; set; } = null!;
}

namespace Cheetah.Identity.Contracts.Requests;

/// <summary>
/// Request to change user password
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

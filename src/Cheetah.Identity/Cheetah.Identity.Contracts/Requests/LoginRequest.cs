namespace Cheetah.Identity.Contracts.Requests;

/// <summary>
/// Request to login
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

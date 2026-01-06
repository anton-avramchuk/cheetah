namespace Cheetah.Identity.Contracts.Requests;

/// <summary>
/// Request to register a new user
/// </summary>
public class RegisterUserRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

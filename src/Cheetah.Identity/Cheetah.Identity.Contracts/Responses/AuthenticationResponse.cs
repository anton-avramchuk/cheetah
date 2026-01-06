namespace Cheetah.Identity.Contracts.Responses;

/// <summary>
/// Response after successful authentication
/// </summary>
public class AuthenticationResponse
{
    public bool Succeeded { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string> Errors { get; set; } = [];
    public UserInfo? User { get; set; }

    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string> Roles { get; set; } = [];
        public List<string> Permissions { get; set; } = [];
    }
}

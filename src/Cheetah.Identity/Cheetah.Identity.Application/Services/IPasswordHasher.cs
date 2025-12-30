namespace Cheetah.Identity.Application.Services;

/// <summary>
/// Service for hashing and verifying passwords
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a password using a secure algorithm (BCrypt, PBKDF2, Argon2)
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verify a password against a hash
    /// </summary>
    bool VerifyPassword(string password, string hash);
}

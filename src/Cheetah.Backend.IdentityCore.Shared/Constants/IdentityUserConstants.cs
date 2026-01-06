namespace Cheetah.Backend.IdentityCore.Shared.Constants;

/// <summary>
/// Constants for IdentityUser entity field lengths
/// </summary>
public static class IdentityUserConstants
{
    public const int MaxUserNameLength = 256;
    public const int MaxNormalizedUserNameLength = 256;
    public const int MaxEmailLength = 256;
    public const int MaxNormalizedEmailLength = 256;
    public const int MaxPasswordHashLength = 1024;
    public const int MaxSecurityStampLength = 1024;
}

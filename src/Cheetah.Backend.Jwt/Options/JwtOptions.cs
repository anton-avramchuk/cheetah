using System.ComponentModel.DataAnnotations;

namespace Cheetah.Backend.Jwt.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32)]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}

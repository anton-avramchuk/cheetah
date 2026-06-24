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

    /// <summary>
    /// Время жизни сервисного (machine-to-machine) токена. Намеренно короткое —
    /// сервис обновляет токен по мере истечения.
    /// </summary>
    public int ServiceTokenExpirationMinutes { get; set; } = 10;
}

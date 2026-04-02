using System.ComponentModel.DataAnnotations;

namespace Cheetah.Backend.Rsa;

public sealed class RsaOptions
{
    public const string SectionName = "Rsa";

    [Required]
    public string PrivateKeyPem { get; set; } = string.Empty;
}

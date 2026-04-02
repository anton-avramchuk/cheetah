namespace Cheetah.Backend.Rsa.Abstractions;

public interface IRsaPublicKeyProvider
{
    string PublicKeyBase64 { get; }
}

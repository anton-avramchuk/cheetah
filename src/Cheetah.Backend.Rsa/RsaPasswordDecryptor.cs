using System.Security.Cryptography;
using System.Text;
using Cheetah.Backend.Rsa.Abstractions;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.Backend.Rsa;

[Export(LifetimeType.Singleton, typeof(IPasswordDecryptor), typeof(IRsaPublicKeyProvider))]
public sealed class RsaPasswordDecryptor : IPasswordDecryptor, IRsaPublicKeyProvider, IDisposable
{
    private readonly RSA _rsa;

    public string PublicKeyBase64 { get; }

    public RsaPasswordDecryptor(IOptions<RsaOptions> options)
    {
        _rsa = RSA.Create();
        _rsa.ImportFromPem(options.Value.PrivateKeyPem);
        PublicKeyBase64 = Convert.ToBase64String(_rsa.ExportSubjectPublicKeyInfo());
    }

    public string Decrypt(string encryptedBase64)
    {
        var bytes = Convert.FromBase64String(encryptedBase64);
        var decrypted = _rsa.Decrypt(bytes, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decrypted);
    }

    public void Dispose() => _rsa.Dispose();
}

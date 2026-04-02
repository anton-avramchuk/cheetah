using Cheetah.Backend.Rsa.Abstractions;

namespace Cheetah.Modules.Identity.Application.Services;

public sealed class PassThroughPasswordDecryptor : IPasswordDecryptor
{
    public string Decrypt(string encryptedPassword) => encryptedPassword;
}

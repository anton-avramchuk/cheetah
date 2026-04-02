namespace Cheetah.Backend.Rsa.Abstractions;

public interface IPasswordDecryptor
{
    string Decrypt(string encryptedPassword);
}

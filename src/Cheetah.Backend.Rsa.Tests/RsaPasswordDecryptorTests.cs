using System.Security.Cryptography;
using System.Text;
using Cheetah.Backend.Rsa;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Cheetah.Backend.Rsa.Tests;

public class RsaPasswordDecryptorTests : IDisposable
{
    private readonly RSA _rsa;
    private readonly RsaPasswordDecryptor _decryptor;

    public RsaPasswordDecryptorTests()
    {
        _rsa = RSA.Create(2048);
        var privatePem = ExportPrivateKeyPem(_rsa);

        var options = Options.Create(new RsaOptions { PrivateKeyPem = privatePem });
        _decryptor = new RsaPasswordDecryptor(options);
    }

    [Fact]
    public void Decrypt_WithValidEncryptedPassword_ShouldReturnPlainText()
    {
        // Arrange
        var plainPassword = "P@ssw0rd!";
        var encrypted = EncryptWithPublicKey(plainPassword);

        // Act
        var result = _decryptor.Decrypt(encrypted);

        // Assert
        result.ShouldBe(plainPassword);
    }

    [Theory]
    [InlineData("простой пароль")]
    [InlineData("password with spaces")]
    [InlineData("!@#$%^&*()")]
    [InlineData("very-long-password-that-still-fits-rsa-2048-oaep-sha256-limit")]
    public void Decrypt_WithVariousPasswords_ShouldRoundTrip(string plainPassword)
    {
        var encrypted = EncryptWithPublicKey(plainPassword);
        _decryptor.Decrypt(encrypted).ShouldBe(plainPassword);
    }

    [Fact]
    public void Decrypt_WithInvalidBase64_ShouldThrowFormatException()
    {
        var act = () => _decryptor.Decrypt("not-valid-base64!!!");
        act.ShouldThrow<FormatException>();
    }

    [Fact]
    public void Decrypt_WithDataEncryptedByDifferentKey_ShouldThrowCryptographicException()
    {
        // Arrange
        using var otherRsa = RSA.Create(2048);
        var plainPassword = "P@ssw0rd!";
        var encryptedWithOtherKey = Convert.ToBase64String(
            otherRsa.Encrypt(Encoding.UTF8.GetBytes(plainPassword), RSAEncryptionPadding.OaepSHA256));

        // Act
        var act = () => _decryptor.Decrypt(encryptedWithOtherKey);

        // Assert
        act.ShouldThrow<CryptographicException>();
    }

    [Fact]
    public void PublicKeyBase64_ShouldBeNonEmpty()
    {
        _decryptor.PublicKeyBase64.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void PublicKeyBase64_ShouldBeValidSpkiKey()
    {
        // Arrange
        var keyBytes = Convert.FromBase64String(_decryptor.PublicKeyBase64);

        // Act
        using var rsa = RSA.Create();
        var act = () => rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

        // Assert — should not throw
        act.ShouldNotThrow();
    }

    [Fact]
    public void PublicKeyBase64_ShouldCorrespondToPrivateKey()
    {
        // Encrypt with decryptor's public key, decrypt with the original private key
        var plainPassword = "test";
        var publicKeyBytes = Convert.FromBase64String(_decryptor.PublicKeyBase64);

        using var rsaPublic = RSA.Create();
        rsaPublic.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

        var encrypted = Convert.ToBase64String(
            rsaPublic.Encrypt(Encoding.UTF8.GetBytes(plainPassword), RSAEncryptionPadding.OaepSHA256));

        _decryptor.Decrypt(encrypted).ShouldBe(plainPassword);
    }

    private string EncryptWithPublicKey(string plainText)
    {
        var publicKeyBytes = Convert.FromBase64String(_decryptor.PublicKeyBase64);
        using var rsaPublic = RSA.Create();
        rsaPublic.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
        var encrypted = rsaPublic.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(encrypted);
    }

    private static string ExportPrivateKeyPem(RSA rsa)
    {
        var privateKeyBytes = rsa.ExportRSAPrivateKey();
        var base64 = Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks);
        return $"-----BEGIN RSA PRIVATE KEY-----\n{base64}\n-----END RSA PRIVATE KEY-----";
    }

    public void Dispose() => _decryptor.Dispose();
}

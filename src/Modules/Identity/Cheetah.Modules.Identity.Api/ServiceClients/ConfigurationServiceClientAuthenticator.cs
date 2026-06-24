using System.Security.Cryptography;
using System.Text;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Services;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Identity.Api.ServiceClients;

/// <summary>
/// Аутентификатор сервисных клиентов на основе конфигурации (<see cref="ServiceClientsOptions"/>).
/// Сравнение секрета — константное по времени, чтобы не утекала длина/совпадение префикса.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IServiceClientAuthenticator))]
internal sealed class ConfigurationServiceClientAuthenticator(IOptions<ServiceClientsOptions> options)
    : IServiceClientAuthenticator
{
    private readonly ServiceClientsOptions _options = options.Value;

    public ServiceClientPrincipal? Authenticate(string clientId, string clientSecret)
    {
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            return null;

        var entry = _options.Clients.FirstOrDefault(c =>
            string.Equals(c.ClientId, clientId, StringComparison.Ordinal));

        if (entry is null || !SecretsMatch(entry.ClientSecret, clientSecret))
            return null;

        return new ServiceClientPrincipal(entry.ClientId, entry.Roles.AsReadOnly());
    }

    private static bool SecretsMatch(string expected, string provided)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}

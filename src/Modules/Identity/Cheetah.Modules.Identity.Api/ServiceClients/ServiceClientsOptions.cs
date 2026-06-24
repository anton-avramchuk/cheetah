namespace Cheetah.Modules.Identity.Api.ServiceClients;

/// <summary>
/// Реестр сервисных клиентов (machine-to-machine), биндится из секции <c>ServiceClients</c>.
/// MVP: секреты в конфигурации. В проде — хранить в БД/секрет-менеджере и сравнивать хэши.
/// </summary>
public sealed class ServiceClientsOptions
{
    public const string SectionName = "ServiceClients";

    public List<ServiceClientEntry> Clients { get; set; } = [];
}

public sealed class ServiceClientEntry
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Роли, которые получит сервисный токен этого клиента.</summary>
    public List<string> Roles { get; set; } = [];
}

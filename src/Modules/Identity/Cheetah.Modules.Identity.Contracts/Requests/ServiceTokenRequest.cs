using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Cheetah.Modules.Identity.Contracts.Requests;

/// <summary>
/// Запрос сервисного токена по схеме client_credentials (machine-to-machine).
/// </summary>
public record ServiceTokenRequest(
    [property: Required(AllowEmptyStrings = false)]
    string ClientId,
    [property: Required(AllowEmptyStrings = false)]
    string ClientSecret) : ICrmRequest;

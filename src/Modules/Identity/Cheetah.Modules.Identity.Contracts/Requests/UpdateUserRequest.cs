using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Cheetah.Modules.Identity.Contracts.Requests;

/// <summary>Базовый запрос обновления пользователя (расширяется хостом).</summary>
public abstract record UpdateUserRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string UserName,
    [property: Required(AllowEmptyStrings = false)]
    [property: EmailAddress]
    string Email,
    IReadOnlyList<Guid>? RoleIds = null) : ICrmRequest;

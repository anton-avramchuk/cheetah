using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Identity.Contracts.Response;

/// <summary>Базовая детальная ViewModel пользователя (расширяется хостом).</summary>
public abstract record UserDetailViewModel(Guid Id, string UserName, string Email, IReadOnlyList<Guid> RoleIds) : ICrmResponse;

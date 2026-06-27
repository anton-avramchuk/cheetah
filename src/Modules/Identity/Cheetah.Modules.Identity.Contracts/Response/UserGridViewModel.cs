using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Identity.Contracts.Response;

/// <summary>Базовая ViewModel строки грида пользователей (расширяется хостом).</summary>
public abstract record UserGridViewModel(Guid Id, string UserName, string Email) : ICrmResponse;

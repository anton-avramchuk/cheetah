using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Identity.Contracts.Response;

/// <summary>Базовая ViewModel строки грида ролей (расширяется хостом).</summary>
public abstract record RoleGridViewModel(Guid Id, string Name) : ICrmResponse;

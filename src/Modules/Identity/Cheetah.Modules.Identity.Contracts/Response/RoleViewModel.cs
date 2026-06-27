using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Identity.Contracts.Response;

/// <summary>Базовая детальная ViewModel роли (расширяется хостом).</summary>
public abstract record RoleViewModel(Guid Id, string Name) : ICrmResponse;

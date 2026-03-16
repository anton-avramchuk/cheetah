using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Identity.Contracts.Response;

public record UserDetailViewModel(Guid Id, string UserName, string Email, IReadOnlyList<Guid> RoleIds) : ICrmResponse;

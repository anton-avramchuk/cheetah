using Cheetah.Contracts.Responses;

namespace Crm.Identity.Contracts.Response;

public record UserDetailViewModel(Guid Id, string UserName, string Email, IReadOnlyList<Guid> RoleIds) : ICrmResponse;

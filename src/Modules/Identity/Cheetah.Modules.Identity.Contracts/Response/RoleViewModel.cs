using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Identity.Contracts.Response;

public record RoleViewModel(Guid Id, string Name) : ICrmResponse;

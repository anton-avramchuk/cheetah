using Cheetah.Contracts.Responses;

namespace Crm.Identity.Contracts.Response;

public record RoleViewModel(Guid Id, string Name) : ICrmResponse;

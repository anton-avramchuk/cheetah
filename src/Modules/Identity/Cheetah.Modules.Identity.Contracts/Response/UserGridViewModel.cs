using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Identity.Contracts.Response;

public record UserGridViewModel(Guid Id, string UserName, string Email) : ICrmResponse;

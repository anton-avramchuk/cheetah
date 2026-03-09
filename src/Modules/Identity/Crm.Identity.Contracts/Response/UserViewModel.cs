using Cheetah.Contracts.Responses;

namespace Crm.Identity.Contracts.Response;

public record UserGridViewModel(Guid Id, string UserName, string Email) : ICrmResponse;

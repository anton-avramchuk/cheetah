using Cheetah.Contracts.Responses;

namespace Crm.Identity.Contracts.Response;

public record UserViewModel(Guid Id, string UserName, string Email, bool EmailConfirmed) : ICrmResponse;

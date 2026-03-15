using Cheetah.Contracts.Responses;

namespace Crm.Customer.Contracts.Response;

public record CustomerViewModel(Guid Id, string Name, string? Description, Guid? IndustryId) : ICrmResponse;
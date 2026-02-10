using Cheetah.Contracts.Responses;

namespace Crm.Recruitment.Contracts.Response;

public record StackItemViewModel(Guid Id, string Name) : ICrmResponse;

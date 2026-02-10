using Cheetah.Contracts.Responses;

namespace Crm.Recruitment.Contracts.Response;

public record PositionViewModel(Guid Id, string Name) : ICrmResponse;

using Cheetah.Contracts.Responses;

namespace Crm.Recruitment.Contracts.Response;

public record CustomerViewModel(Guid Id, string Name, string? Description, Guid? DirectionId) : ICrmResponse;

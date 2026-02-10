using Cheetah.Contracts.Responses;

namespace Crm.Recruitment.Contracts.Response;

public record CustomerDirectionViewModel(Guid Id, string Name, string? Description) : ICrmResponse;

using Cheetah.Contracts.Responses;

namespace Crm.Recruitment.Contracts.Response;

public record VacancyViewModel(Guid Id, string Name, string? Description) : ICrmResponse;
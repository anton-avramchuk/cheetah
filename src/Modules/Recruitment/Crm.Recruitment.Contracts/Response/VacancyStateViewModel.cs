using Cheetah.Contracts.Responses;

namespace Crm.Recruitment.Contracts.Response;

public record VacancyStateViewModel(Guid Id, string Name, int Order, string? Color, bool IsDefault) : ICrmResponse;

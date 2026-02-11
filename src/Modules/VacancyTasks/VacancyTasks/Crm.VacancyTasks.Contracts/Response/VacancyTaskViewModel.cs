using Cheetah.Contracts.Responses;

namespace Crm.VacancyTasks.Contracts.Response;

public record VacancyTaskViewModel(Guid Id, string Name, string? Description) : ICrmResponse;
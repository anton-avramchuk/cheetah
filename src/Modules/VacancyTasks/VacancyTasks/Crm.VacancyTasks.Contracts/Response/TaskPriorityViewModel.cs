using Cheetah.Contracts.Responses;

namespace Crm.VacancyTasks.Contracts.Response;

public record TaskPriorityViewModel(Guid Id, string Name, int Order, string? Color) : ICrmResponse;

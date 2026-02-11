using Cheetah.Contracts.Responses;

namespace Crm.VacancyTasks.Contracts.Response;

public record VacancyTaskViewModel(
    Guid Id,
    string Title,
    string? Description,
    Guid VacancyId,
    Guid StateId,
    Guid? PriorityId,
    Guid? AssigneeId,
    DateTimeOffset? DueDate,
    int Order) : ICrmResponse;

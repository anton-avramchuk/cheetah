using Cheetah.Contracts.Responses;

namespace Crm.VacancyTasks.Contracts.Response;

public record VacancyTaskViewModel(
    Guid Id,
    string Title,
    string? Description,
    Guid VacancyId,
    Guid StateId,
    string? StateName,
    Guid? PriorityId,
    string? PriorityName,
    string? PriorityColor,
    Guid? AssigneeId,
    DateTimeOffset? DueDate,
    int Order,
    int Number) : ICrmResponse;

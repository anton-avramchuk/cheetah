namespace Crm.VacancyTasks.Application;

public record VacancyTaskModel(
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
    int Number);

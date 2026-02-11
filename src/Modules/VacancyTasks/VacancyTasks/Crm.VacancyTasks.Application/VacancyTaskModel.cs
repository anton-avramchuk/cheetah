namespace Crm.VacancyTasks.Application;

public record VacancyTaskModel(
    Guid Id,
    string Title,
    string? Description,
    Guid VacancyId,
    Guid StateId,
    Guid? PriorityId,
    Guid? AssigneeId,
    DateTimeOffset? DueDate,
    int Order);

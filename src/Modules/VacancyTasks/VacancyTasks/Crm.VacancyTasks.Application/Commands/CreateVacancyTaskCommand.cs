using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record CreateVacancyTaskCommand(
    string Title,
    Guid VacancyId,
    Guid StateId,
    string? Description,
    Guid? PriorityId,
    Guid? AssigneeId,
    DateTimeOffset? DueDate) : ICommand<Guid>;
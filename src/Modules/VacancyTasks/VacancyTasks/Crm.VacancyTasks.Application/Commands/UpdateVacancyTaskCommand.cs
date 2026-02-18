using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record UpdateVacancyTaskCommand(Guid Id, string Title, string? Description, Guid StateId, Guid? PriorityId, DateTimeOffset? DueDate) : ICommand;
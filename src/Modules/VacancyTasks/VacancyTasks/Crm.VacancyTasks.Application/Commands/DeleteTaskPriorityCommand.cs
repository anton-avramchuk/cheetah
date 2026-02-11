using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record DeleteTaskPriorityCommand(Guid Id) : ICommand;

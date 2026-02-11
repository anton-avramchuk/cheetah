using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record UpdateTaskPriorityCommand(Guid Id, string Name, int Order, string? Color) : ICommand;

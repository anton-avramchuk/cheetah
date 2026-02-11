using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record CreateTaskPriorityCommand(string Name, int Order, string? Color) : ICommand<Guid>;

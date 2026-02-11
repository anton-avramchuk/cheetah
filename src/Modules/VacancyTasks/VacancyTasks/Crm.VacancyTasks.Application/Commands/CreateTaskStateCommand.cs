using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record CreateTaskStateCommand(string Name, int Order, string? Color, bool IsDefault) : ICommand<Guid>;

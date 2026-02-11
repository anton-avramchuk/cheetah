using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record UpdateTaskStateCommand(Guid Id, string Name, int Order, string? Color, bool IsDefault) : ICommand;

using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record UpdateVacancyTaskCommand(Guid Id, string Name, string? Description) : ICommand;
using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record UpdateVacancyCommand(Guid Id, string Name, string? Description) : ICommand;
using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record DeleteVacancyCommand(Guid Id) : ICommand;
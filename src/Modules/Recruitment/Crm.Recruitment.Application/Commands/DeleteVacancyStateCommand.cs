using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record DeleteVacancyStateCommand(Guid Id) : ICommand;

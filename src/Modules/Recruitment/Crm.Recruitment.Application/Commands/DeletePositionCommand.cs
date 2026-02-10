using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record DeletePositionCommand(Guid Id) : ICommand;

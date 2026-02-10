using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record UpdatePositionCommand(Guid Id, string Name) : ICommand;

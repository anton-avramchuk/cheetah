using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record UpdateStackItemCommand(Guid Id, string Name) : ICommand;

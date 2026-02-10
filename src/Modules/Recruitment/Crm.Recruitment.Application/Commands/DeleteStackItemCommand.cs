using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record DeleteStackItemCommand(Guid Id) : ICommand;

using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record UpdateStackItemCommand(Guid Id, string Name, string? Description) : ICommand;
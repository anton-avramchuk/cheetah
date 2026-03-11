using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record CreateStackItemCommand(string Name, string? Description) : ICommand<Guid>;
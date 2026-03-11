using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record CreateWorkFormatCommand(string Name) : ICommand<Guid>;

using Cheetah.Core.CQRS;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Application.Commands;

public record UpdatePositionCommand(Guid Id, string Name, Grade Grade) : ICommand;

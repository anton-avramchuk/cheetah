using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record DeleteSkillCommand(Guid Id) : ICommand;

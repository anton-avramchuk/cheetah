using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record UpdateSkillCategoryCommand(Guid Id, string Name) : ICommand;

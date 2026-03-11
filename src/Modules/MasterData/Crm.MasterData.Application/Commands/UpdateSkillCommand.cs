using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record UpdateSkillCommand(Guid Id, string Name, Guid? SkillCategoryId) : ICommand;

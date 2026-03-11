using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record CreateSkillCommand(string Name, Guid? SkillCategoryId) : ICommand<Guid>;

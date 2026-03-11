using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record CreateSkillCategoryCommand(string Name) : ICommand<Guid>;

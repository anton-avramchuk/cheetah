using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record DeleteSkillCategoryCommand(Guid Id) : ICommand;

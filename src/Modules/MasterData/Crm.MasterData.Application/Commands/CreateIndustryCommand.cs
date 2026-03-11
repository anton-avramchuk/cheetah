using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record CreateIndustryCommand(string Name) : ICommand<Guid>;

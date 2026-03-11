using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record UpdateIndustryCommand(Guid Id, string Name) : ICommand;

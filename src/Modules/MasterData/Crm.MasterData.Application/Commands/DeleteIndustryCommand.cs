using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record DeleteIndustryCommand(Guid Id) : ICommand;

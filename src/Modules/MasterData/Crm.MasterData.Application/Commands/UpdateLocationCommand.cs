using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record UpdateLocationCommand(Guid Id, string Country, string City, string Timezone) : ICommand;

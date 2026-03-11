using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record CreateLocationCommand(string Country, string City, string Timezone) : ICommand<Guid>;

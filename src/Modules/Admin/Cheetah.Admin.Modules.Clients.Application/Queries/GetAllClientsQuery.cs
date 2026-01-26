using Cheetah.Core.CQRS;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

public class GetAllClientsQuery : IQuery<IReadOnlyList<ClientModel>>
{
}
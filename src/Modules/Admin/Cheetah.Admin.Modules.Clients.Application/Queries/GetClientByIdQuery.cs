using Cheetah.Core.CQRS;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

public record GetClientByIdQuery(Guid Id) : IQuery<ClientModel?>;

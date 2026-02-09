using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.Logging;

namespace Cheetah.Admin.Modules.Clients.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Client>), typeof(IClientRepository), typeof(IRepository<Client, Guid>))]
public class ClientRepository : EfGridRepository<ClientsDbContext, Client>, IClientRepository
{
    public ClientRepository(ClientsDbContext context, IObjectMapper mapper, ILogger<ClientRepository> logger)
        : base(context, mapper, logger)
    {
    }
}

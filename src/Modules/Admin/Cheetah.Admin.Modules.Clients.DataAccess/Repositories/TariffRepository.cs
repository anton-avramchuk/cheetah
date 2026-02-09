using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Core.Specification;
using Cheetah.Mapping.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cheetah.Admin.Modules.Clients.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Tariff>), typeof(ITariffRepository), typeof(IRepository<Tariff, Guid>))]
public class TariffRepository : EfGridRepository<ClientsDbContext, Tariff>, ITariffRepository
{
    public TariffRepository(ClientsDbContext context, IObjectMapper mapper, ILogger<TariffRepository> logger)
        : base(context, mapper, logger)
    {
    }

    public async ValueTask<Tariff?> GetByIdNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async ValueTask<List<Tariff>> GetAllNoTrackingAsync(ISpecification<Tariff>? spec = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(cancellationToken);
    }
}

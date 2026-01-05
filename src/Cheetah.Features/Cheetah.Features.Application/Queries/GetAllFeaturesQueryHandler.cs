using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Features.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Features.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllFeaturesQuery, IReadOnlyList<Feature>>))]
public class GetAllFeaturesQueryHandler(
    IRepository<Feature, string> repository
) : IQueryHandler<GetAllFeaturesQuery, IReadOnlyList<Feature>>
{
    public async ValueTask<IReadOnlyList<Feature>> HandleAsync(GetAllFeaturesQuery query, CancellationToken cancellationToken = default)
    {
        return await repository.GetQuery()
            .AsNoTracking()
            .OrderBy(f => f.Group)
            .ThenBy(f => f.DisplayName)
            .ToListAsync(cancellationToken);
    }
}

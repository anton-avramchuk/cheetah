using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Features.Domain.Entities;

namespace Cheetah.Features.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllFeaturesQuery, IReadOnlyList<Feature>>))]
public class GetAllFeaturesQueryHandler(
    IReadOnlyRepository<Feature, string> repository
) : IQueryHandler<GetAllFeaturesQuery, IReadOnlyList<Feature>>
{
    public async ValueTask<IReadOnlyList<Feature>> HandleAsync(GetAllFeaturesQuery query, CancellationToken cancellationToken = default)
    {
        var features = await repository.GetAllAsync(cancellationToken);

        // Sort in memory (for simplicity)
        return features
            .OrderBy(f => f.Group)
            .ThenBy(f => f.DisplayName)
            .ToList();
    }
}

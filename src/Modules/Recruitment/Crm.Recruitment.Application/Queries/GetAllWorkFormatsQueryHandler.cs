using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllWorkFormatsQuery, IReadOnlyList<WorkFormatModel>>))]
public class GetAllWorkFormatsQueryHandler : IQueryHandler<GetAllWorkFormatsQuery, IReadOnlyList<WorkFormatModel>>
{
    private readonly IReadOnlyRepository<WorkFormat, Guid> _repository;

    public GetAllWorkFormatsQueryHandler(IRepository<WorkFormat, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<WorkFormatModel>> HandleAsync(
        GetAllWorkFormatsQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(e => e.Name)
            .ToListAsync(ct);

        return entities
            .Select(e => new WorkFormatModel(e.Id, e.Name))
            .ToList();
    }
}

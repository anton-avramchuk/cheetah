using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetWorkFormatByIdQuery, WorkFormatModel?>))]
public class GetWorkFormatByIdQueryHandler : IQueryHandler<GetWorkFormatByIdQuery, WorkFormatModel?>
{
    private readonly IReadOnlyRepository<WorkFormat, Guid> _repository;

    public GetWorkFormatByIdQueryHandler(IRepository<WorkFormat, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<WorkFormatModel?> HandleAsync(GetWorkFormatByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new WorkFormatModel(entity.Id, entity.Name);
    }
}

using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetVacancyStateByIdQuery, VacancyStateModel?>))]
public class GetVacancyStateByIdQueryHandler : IQueryHandler<GetVacancyStateByIdQuery, VacancyStateModel?>
{
    private readonly IReadOnlyRepository<VacancyState, Guid> _repository;

    public GetVacancyStateByIdQueryHandler(IRepository<VacancyState, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<VacancyStateModel?> HandleAsync(GetVacancyStateByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new VacancyStateModel(entity.Id, entity.Name, entity.Order, entity.Color?.Value, entity.IsDefault);
    }
}

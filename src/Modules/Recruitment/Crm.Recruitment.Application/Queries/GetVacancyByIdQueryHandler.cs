using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetVacancyByIdQuery, VacancyModel?>))]
public class GetVacancyByIdQueryHandler : IQueryHandler<GetVacancyByIdQuery, VacancyModel?>
{
    private readonly IReadOnlyRepository<Vacancy, Guid> _repository;

    public GetVacancyByIdQueryHandler(IRepository<Vacancy, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<VacancyModel?> HandleAsync(GetVacancyByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .Include(e => e.State)
            .Include(e => e.Customer)
            .Include(e => e.Position)
            .Include(e => e.StackItem)
            .Include(e => e.WorkFormat)
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new VacancyModel(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.StateId,
            entity.State?.Name,
            entity.CustomerId,
            entity.Customer?.Name,
            entity.PositionId,
            entity.Position?.Name,
            entity.StackItemId,
            entity.StackItem?.Name,
            entity.WorkFormatId,
            entity.WorkFormat?.Name,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}

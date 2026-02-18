using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetVacancyTaskByIdQuery, VacancyTaskModel?>))]
public class GetVacancyTaskByIdQueryHandler : IQueryHandler<GetVacancyTaskByIdQuery, VacancyTaskModel?>
{
    private readonly IReadOnlyRepository<VacancyTask, Guid> _repository;

    public GetVacancyTaskByIdQueryHandler(IRepository<VacancyTask, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<VacancyTaskModel?> HandleAsync(GetVacancyTaskByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .Include(e => e.State)
            .Include(e => e.Priority)
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new VacancyTaskModel(
            entity.Id, entity.Title, entity.Description, entity.VacancyId,
            entity.StateId, entity.State?.Name,
            entity.PriorityId, entity.Priority?.Name, entity.Priority?.Color?.Value,
            entity.AssigneeId, entity.DueDate, entity.Order, entity.Number);
    }
}
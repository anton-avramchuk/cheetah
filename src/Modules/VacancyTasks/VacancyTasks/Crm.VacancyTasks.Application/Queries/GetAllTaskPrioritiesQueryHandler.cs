using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllTaskPrioritiesQuery, IReadOnlyList<TaskPriorityModel>>))]
public class GetAllTaskPrioritiesQueryHandler : IQueryHandler<GetAllTaskPrioritiesQuery, IReadOnlyList<TaskPriorityModel>>
{
    private readonly IReadOnlyRepository<TaskPriority, Guid> _repository;

    public GetAllTaskPrioritiesQueryHandler(IRepository<TaskPriority, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<TaskPriorityModel>> HandleAsync(
        GetAllTaskPrioritiesQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(e => e.Order)
            .ToListAsync(ct);

        return entities
            .Select(e => new TaskPriorityModel(e.Id, e.Name, e.Order, e.Color))
            .ToList();
    }
}

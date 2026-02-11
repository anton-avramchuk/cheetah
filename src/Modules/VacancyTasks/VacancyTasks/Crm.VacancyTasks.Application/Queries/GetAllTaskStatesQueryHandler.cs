using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllTaskStatesQuery, IReadOnlyList<TaskStateModel>>))]
public class GetAllTaskStatesQueryHandler : IQueryHandler<GetAllTaskStatesQuery, IReadOnlyList<TaskStateModel>>
{
    private readonly IReadOnlyRepository<TaskState, Guid> _repository;

    public GetAllTaskStatesQueryHandler(IRepository<TaskState, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<TaskStateModel>> HandleAsync(
        GetAllTaskStatesQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(e => e.Order)
            .ToListAsync(ct);

        return entities
            .Select(e => new TaskStateModel(e.Id, e.Name, e.Order, e.Color, e.IsDefault))
            .ToList();
    }
}

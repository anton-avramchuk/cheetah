using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTaskStateByIdQuery, TaskStateModel?>))]
public class GetTaskStateByIdQueryHandler : IQueryHandler<GetTaskStateByIdQuery, TaskStateModel?>
{
    private readonly IReadOnlyRepository<TaskState, Guid> _repository;

    public GetTaskStateByIdQueryHandler(IRepository<TaskState, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<TaskStateModel?> HandleAsync(GetTaskStateByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new TaskStateModel(entity.Id, entity.Name, entity.Order, entity.Color, entity.IsDefault);
    }
}

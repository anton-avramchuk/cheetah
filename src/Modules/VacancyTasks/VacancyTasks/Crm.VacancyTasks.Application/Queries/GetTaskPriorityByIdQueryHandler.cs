using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.VacancyTasks.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTaskPriorityByIdQuery, TaskPriorityModel?>))]
public class GetTaskPriorityByIdQueryHandler : IQueryHandler<GetTaskPriorityByIdQuery, TaskPriorityModel?>
{
    private readonly IReadOnlyRepository<TaskPriority, Guid> _repository;

    public GetTaskPriorityByIdQueryHandler(IRepository<TaskPriority, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<TaskPriorityModel?> HandleAsync(GetTaskPriorityByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new TaskPriorityModel(entity.Id, entity.Name, entity.Order, entity.Color);
    }
}

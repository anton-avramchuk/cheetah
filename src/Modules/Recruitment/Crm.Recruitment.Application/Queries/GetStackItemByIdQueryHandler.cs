using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetStackItemByIdQuery, StackItemModel?>))]
public class GetStackItemByIdQueryHandler : IQueryHandler<GetStackItemByIdQuery, StackItemModel?>
{
    private readonly IReadOnlyRepository<StackItem, Guid> _repository;

    public GetStackItemByIdQueryHandler(IRepository<StackItem, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<StackItemModel?> HandleAsync(GetStackItemByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new StackItemModel(entity.Id, entity.Name);
    }
}

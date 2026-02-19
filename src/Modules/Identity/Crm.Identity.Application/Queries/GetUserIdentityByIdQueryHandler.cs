using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserIdentityByIdQuery, UserIdentityModel?>))]
public class GetUserIdentityByIdQueryHandler : IQueryHandler<GetUserIdentityByIdQuery, UserIdentityModel?>
{
    private readonly IReadOnlyRepository<UserIdentity, Guid> _repository;

    public GetUserIdentityByIdQueryHandler(IRepository<UserIdentity, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<UserIdentityModel?> HandleAsync(GetUserIdentityByIdQuery query,
        CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new UserIdentityModel(entity.Id, entity.Name, entity.Description);
    }
}
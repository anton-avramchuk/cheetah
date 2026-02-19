using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Identity.Domain;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteUserIdentityCommand>))]
public class DeleteUserIdentityCommandHandler : ICommandHandler<DeleteUserIdentityCommand>
{
    private readonly IRepository<UserIdentity, Guid> _repository;

    public DeleteUserIdentityCommandHandler(IRepository<UserIdentity, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteUserIdentityCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<UserIdentity>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeletePositionCommand>))]
public class DeletePositionCommandHandler : ICommandHandler<DeletePositionCommand>
{
    private readonly IRepository<Position, Guid> _repository;

    public DeletePositionCommandHandler(IRepository<Position, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeletePositionCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Position>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}

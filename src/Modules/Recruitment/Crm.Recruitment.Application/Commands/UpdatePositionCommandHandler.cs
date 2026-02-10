using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdatePositionCommand>))]
public class UpdatePositionCommandHandler : ICommandHandler<UpdatePositionCommand>
{
    private readonly IRepository<Position, Guid> _repository;

    public UpdatePositionCommandHandler(IRepository<Position, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UpdatePositionCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<Position>(command.Id);

        entity.Update(command.Name);
        await _repository.SaveChangesAsync(ct);
    }
}

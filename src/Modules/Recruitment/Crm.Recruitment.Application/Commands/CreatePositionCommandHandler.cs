using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreatePositionCommand, Guid>))]
public class CreatePositionCommandHandler : ICommandHandler<CreatePositionCommand, Guid>
{
    private readonly IRepository<Position, Guid> _repository;

    public CreatePositionCommandHandler(IRepository<Position, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreatePositionCommand command, CancellationToken ct = default)
    {
        var entity = Position.Create(command.Name);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}

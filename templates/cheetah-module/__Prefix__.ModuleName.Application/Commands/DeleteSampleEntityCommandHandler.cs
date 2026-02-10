using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using __Prefix__.ModuleName.Domain;

namespace __Prefix__.ModuleName.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteSampleEntityCommand>))]
public class DeleteSampleEntityCommandHandler : ICommandHandler<DeleteSampleEntityCommand>
{
    private readonly IRepository<SampleEntity, Guid> _repository;

    public DeleteSampleEntityCommandHandler(IRepository<SampleEntity, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteSampleEntityCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<SampleEntity>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}

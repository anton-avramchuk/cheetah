using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteStackItemCommand>))]
public class DeleteStackItemCommandHandler : ICommandHandler<DeleteStackItemCommand>
{
    private readonly IRepository<StackItem, Guid> _repository;

    public DeleteStackItemCommandHandler(IRepository<StackItem, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteStackItemCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<StackItem>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}

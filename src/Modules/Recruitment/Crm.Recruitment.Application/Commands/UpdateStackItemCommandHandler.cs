using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateStackItemCommand>))]
public class UpdateStackItemCommandHandler : ICommandHandler<UpdateStackItemCommand>
{
    private readonly IRepository<StackItem, Guid> _repository;

    public UpdateStackItemCommandHandler(IRepository<StackItem, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UpdateStackItemCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<StackItem>(command.Id);

        entity.Update(command.Name);
        await _repository.SaveChangesAsync(ct);
    }
}

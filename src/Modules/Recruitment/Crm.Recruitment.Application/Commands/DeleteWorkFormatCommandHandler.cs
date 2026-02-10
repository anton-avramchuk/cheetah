using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteWorkFormatCommand>))]
public class DeleteWorkFormatCommandHandler : ICommandHandler<DeleteWorkFormatCommand>
{
    private readonly IRepository<WorkFormat, Guid> _repository;

    public DeleteWorkFormatCommandHandler(IRepository<WorkFormat, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteWorkFormatCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<WorkFormat>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}

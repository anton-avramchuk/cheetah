using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateWorkFormatCommand>))]
public class UpdateWorkFormatCommandHandler : ICommandHandler<UpdateWorkFormatCommand>
{
    private readonly IRepository<WorkFormat, Guid> _repository;

    public UpdateWorkFormatCommandHandler(IRepository<WorkFormat, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UpdateWorkFormatCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<WorkFormat>(command.Id);

        entity.Update(command.Name);
        await _repository.SaveChangesAsync(ct);
    }
}

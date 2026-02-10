using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateWorkFormatCommand, Guid>))]
public class CreateWorkFormatCommandHandler : ICommandHandler<CreateWorkFormatCommand, Guid>
{
    private readonly IRepository<WorkFormat, Guid> _repository;

    public CreateWorkFormatCommandHandler(IRepository<WorkFormat, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreateWorkFormatCommand command, CancellationToken ct = default)
    {
        var entity = WorkFormat.Create(command.Name);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}

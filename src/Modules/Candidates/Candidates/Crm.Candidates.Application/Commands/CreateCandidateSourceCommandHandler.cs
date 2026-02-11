using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCandidateSourceCommand, Guid>))]
public class CreateCandidateSourceCommandHandler : ICommandHandler<CreateCandidateSourceCommand, Guid>
{
    private readonly IRepository<CandidateSource, Guid> _repository;

    public CreateCandidateSourceCommandHandler(IRepository<CandidateSource, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreateCandidateSourceCommand command, CancellationToken ct = default)
    {
        var entity = CandidateSource.Create(command.Name, command.Order, command.Color);
        _repository.Add(entity);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }
}

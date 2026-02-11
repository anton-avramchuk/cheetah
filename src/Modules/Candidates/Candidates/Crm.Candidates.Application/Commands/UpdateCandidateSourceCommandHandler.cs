using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Candidates.Domain;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateCandidateSourceCommand>))]
public class UpdateCandidateSourceCommandHandler : ICommandHandler<UpdateCandidateSourceCommand>
{
    private readonly IRepository<CandidateSource, Guid> _repository;

    public UpdateCandidateSourceCommandHandler(IRepository<CandidateSource, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UpdateCandidateSourceCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CandidateSource>(command.Id);

        entity.Update(command.Name, command.Order, command.Color);
        await _repository.SaveChangesAsync(ct);
    }
}

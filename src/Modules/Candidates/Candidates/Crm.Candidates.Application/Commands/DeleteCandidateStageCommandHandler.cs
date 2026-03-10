using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Crm.Candidates.Domain;
using Crm.Candidates.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteCandidateStageCommand>))]
public class DeleteCandidateStageCommandHandler : ICommandHandler<DeleteCandidateStageCommand>
{
    private readonly IRepository<CandidateStage, Guid> _repository;
    private readonly IReadOnlyRepository<CandidateApplication, Guid> _applicationRepository;

    public DeleteCandidateStageCommandHandler(
        IRepository<CandidateStage, Guid> repository,
        IRepository<CandidateApplication, Guid> applicationRepository)
    {
        _repository = repository;
        _applicationRepository = applicationRepository;
    }

    public async ValueTask HandleAsync(DeleteCandidateStageCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
                     ?? throw EntityNotFoundException.For<CandidateStage>(command.Id);

        var isInUse = await _applicationRepository.AsNoTrackingQueryable()
            .Where(new CandidateApplicationByStageIdSpecification(command.Id)).AnyAsync(ct);

        if (isInUse)
            throw new InvalidOperationException($"Cannot delete CandidateStage '{entity.Name}' because it is in use by one or more candidate applications.");

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}

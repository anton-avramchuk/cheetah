using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain.Repositories;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UnassignUserFromVacancyCommand>))]
public class UnassignUserFromVacancyCommandHandler : ICommandHandler<UnassignUserFromVacancyCommand>
{
    private readonly IVacancyAssignmentRepository _repository;

    public UnassignUserFromVacancyCommandHandler(IVacancyAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(UnassignUserFromVacancyCommand command, CancellationToken ct = default)
    {
        var assignment = await _repository.GetByIdAsync(command.AssignmentId, ct);
        if (assignment is null)
            throw new InvalidOperationException($"Assignment with id '{command.AssignmentId}' not found.");

        _repository.Delete(assignment);
        await _repository.SaveChangesAsync(ct);
    }
}

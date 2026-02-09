using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Repositories;

namespace Crm.Recruitment.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AssignUserToVacancyCommand, Guid>))]
public class AssignUserToVacancyCommandHandler : ICommandHandler<AssignUserToVacancyCommand, Guid>
{
    private readonly IVacancyAssignmentRepository _assignmentRepository;
    private readonly IRepository<Vacancy, Guid> _vacancyRepository;
    private readonly IRepository<VacancyRole, Guid> _roleRepository;
    private readonly IRepository<User, Guid> _userRepository;

    public AssignUserToVacancyCommandHandler(
        IVacancyAssignmentRepository assignmentRepository,
        IRepository<Vacancy, Guid> vacancyRepository,
        IRepository<VacancyRole, Guid> roleRepository,
        IRepository<User, Guid> userRepository)
    {
        _assignmentRepository = assignmentRepository;
        _vacancyRepository = vacancyRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
    }

    public async ValueTask<Guid> HandleAsync(AssignUserToVacancyCommand command, CancellationToken ct = default)
    {
        // Validate vacancy exists
        var vacancy = await _vacancyRepository.GetByIdAsync(command.VacancyId, ct);
        if (vacancy is null)
            throw new InvalidOperationException($"Vacancy with id '{command.VacancyId}' not found.");

        // Validate user exists
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user is null)
            throw new InvalidOperationException($"User with id '{command.UserId}' not found.");

        // Validate role exists
        var role = await _roleRepository.GetByIdAsync(command.RoleId, ct);
        if (role is null)
            throw new InvalidOperationException($"Role with id '{command.RoleId}' not found.");

        // Check if assignment already exists
        var alreadyAssigned = await _assignmentRepository.ExistsAsync(
            command.VacancyId,
            command.UserId,
            command.RoleId,
            ct);

        if (alreadyAssigned)
            throw new InvalidOperationException(
                $"User is already assigned to this vacancy with role '{role.Name}'.");

        // Validate IsSingle constraint
        if (role.IsSingle)
        {
            var existingCount = await _assignmentRepository.CountByVacancyAndRoleAsync(
                command.VacancyId,
                command.RoleId,
                ct);

            if (existingCount > 0)
                throw new InvalidOperationException(
                    $"Role '{role.Name}' allows only one assignment per vacancy. " +
                    "Remove the existing assignment first.");
        }

        var assignment = VacancyAssignment.Create(command.VacancyId, command.UserId, command.RoleId);
        _assignmentRepository.Add(assignment);
        await _assignmentRepository.SaveChangesAsync(ct);

        return assignment.Id;
    }
}

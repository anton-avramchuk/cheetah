using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record AssignUserToVacancyCommand(
    Guid VacancyId,
    Guid UserId,
    Guid RoleId) : ICommand<Guid>;

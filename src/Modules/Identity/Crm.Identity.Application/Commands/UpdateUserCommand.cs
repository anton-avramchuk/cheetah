using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record UpdateUserCommand(Guid Id, string UserName, string Email, IReadOnlyList<Guid>? RoleIds = null) : ICommand;

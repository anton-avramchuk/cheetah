using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record UpdateUserIdentityCommand(Guid Id, string Name, string? Description) : ICommand;
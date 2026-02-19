using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record CreateUserIdentityCommand(string Name, string? Description) : ICommand<Guid>;
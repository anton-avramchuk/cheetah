using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Application.Commands;

/// <summary>
/// Command to register a new user
/// </summary>
public record RegisterUserCommand(
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null
) : ICommand<Guid>;

using Cheetah.Core.CQRS;
using Cheetah.Identity.Contracts.ViewModels;

namespace Cheetah.Identity.Frontend.Client.Commands;

public record RegisterUserCommand(
    string Email,
    string Password,
    string? FirstName,
    string? LastName
) : ICommand<UserViewModel?>;

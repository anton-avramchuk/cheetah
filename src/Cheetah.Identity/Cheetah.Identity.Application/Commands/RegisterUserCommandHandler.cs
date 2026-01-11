using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Application.Services;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using Cheetah.Identity.Domain.Specifications;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<RegisterUserCommand, Guid>))]
public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventBus _eventBus;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IEventBus eventBus)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(RegisterUserCommand command, CancellationToken ct)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(command.Email, ct);

        if (existingUser != null)
            throw new InvalidOperationException($"User with email {command.Email} already exists");

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(command.Password);

        // Create user
        var user = User.Create(
            command.Email,
            passwordHash,
            command.FirstName,
            command.LastName
        );

        // TODO: Assign default "User" role
        // var defaultRole = await _roleRepository.GetByNameAsync("User", ct);
        // if (defaultRole != null)
        //     user.AddRole(defaultRole);

        _userRepository.Add(user);
        await _userRepository.SaveChangesAsync(ct);

        // Publish domain events
        foreach (var domainEvent in user.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, ct);
        }
        user.ClearDomainEvents();

        return user.Id;
    }
}

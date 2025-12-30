using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Application.Services;
using Cheetah.Identity.DataAccess;
using Cheetah.Identity.Domain.Entities;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<RegisterUserCommand, Guid>))]
public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventBus _eventBus;

    public RegisterUserCommandHandler(
        IIdentityDbContext dbContext,
        IPasswordHasher passwordHasher,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(RegisterUserCommand command, CancellationToken ct)
    {
        // Check if user already exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == command.Email.ToUpperInvariant(), ct);

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
        // var defaultRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User", ct);
        // if (defaultRole != null)
        //     user.AddRole(defaultRole.Id);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        // Publish domain events
        foreach (var domainEvent in user.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, ct);
        }
        user.ClearDomainEvents();

        return user.Id;
    }
}

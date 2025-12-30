using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Application.Services;
using Cheetah.Identity.DataAccess;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<ChangePasswordCommand>))]
public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventBus _eventBus;

    public ChangePasswordCommandHandler(
        IIdentityDbContext dbContext,
        IPasswordHasher passwordHasher,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(ChangePasswordCommand command, CancellationToken ct)
    {
        var user = await _dbContext.Users.FindAsync([command.UserId], ct);
        if (user == null)
            throw new InvalidOperationException($"User {command.UserId} not found");

        // Verify current password
        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect");

        // Hash new password
        var newPasswordHash = _passwordHasher.HashPassword(command.NewPassword);

        // Change password
        user.ChangePassword(newPasswordHash);

        await _dbContext.SaveChangesAsync(ct);

        // Publish domain events
        foreach (var domainEvent in user.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, ct);
        }
        user.ClearDomainEvents();
    }
}

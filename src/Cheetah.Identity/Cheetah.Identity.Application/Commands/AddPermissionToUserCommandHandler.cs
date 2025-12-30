using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.DataAccess;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AddPermissionToUserCommand>))]
public class AddPermissionToUserCommandHandler : ICommandHandler<AddPermissionToUserCommand>
{
    private readonly IIdentityDbContext _dbContext;

    public AddPermissionToUserCommandHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask HandleAsync(AddPermissionToUserCommand command, CancellationToken ct)
    {
        var user = await _dbContext.Users.FindAsync([command.UserId], ct);
        if (user == null)
            throw new InvalidOperationException($"User {command.UserId} not found");

        // Add personal permission to user
        user.AddPermission(command.Permission);

        await _dbContext.SaveChangesAsync(ct);
    }
}

using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Identity.DataAccess.Exceptions;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateUserCommand, Guid>))]
public class CreateUserCommandHandler(UserManager<CrmUser> userManager)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        var user = CrmUser.Create(command.UserName, command.Email);
        var result = await userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
            throw new IdentityException(result);

        return user.Id;
    }
}

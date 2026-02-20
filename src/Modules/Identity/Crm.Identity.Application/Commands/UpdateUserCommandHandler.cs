using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Identity.DataAccess.Exceptions;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateUserCommand>))]
public class UpdateUserCommandHandler(UserManager<CrmUser> userManager)
    : ICommandHandler<UpdateUserCommand>
{
    public async ValueTask HandleAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<CrmUser>(command.Id);

        user.ChangeUserName(command.UserName);
        user.ChangeEmail(command.Email);
        user.RefreshSecurityStamp();

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }
}

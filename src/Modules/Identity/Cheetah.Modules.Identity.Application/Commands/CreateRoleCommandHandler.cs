using Cheetah.Core.CQRS;
using Cheetah.Modules.Identity.Application.Abstractions;
using Cheetah.Modules.Identity.Infrastructure.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

/// <summary>
/// Обобщённый хендлер создания роли. Сборку доменной роли поставляет хост через
/// <see cref="ICreateRoleFactory{TRole,TCommand}"/>.
/// </summary>
public sealed class CreateRoleCommandHandler<TRole, TCommand>(
    RoleManager<TRole> roleManager,
    ICreateRoleFactory<TRole, TCommand> factory)
    : ICommandHandler<TCommand, Guid>
    where TCommand : CreateRoleCommand
    where TRole : IdentityRole
{
    public async ValueTask<Guid> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var role = factory.Create(command);
        var result = await roleManager.CreateAsync(role);

        if (!result.Succeeded)
            throw new IdentityException(result);

        return role.Id;
    }
}

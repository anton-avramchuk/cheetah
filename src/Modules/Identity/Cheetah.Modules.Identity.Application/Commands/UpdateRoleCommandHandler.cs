using Cheetah.Core.CQRS;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Identity.Application.Abstractions;
using Cheetah.Modules.Identity.Infrastructure.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Commands;

/// <summary>
/// Обобщённый хендлер обновления роли. Применение изменений поставляет хост через
/// <see cref="IUpdateRoleApplier{TRole,TCommand}"/>.
/// </summary>
public sealed class UpdateRoleCommandHandler<TRole, TCommand>(
    RoleManager<TRole> roleManager,
    IUpdateRoleApplier<TRole, TCommand> applier)
    : ICommandHandler<TCommand>
    where TCommand : UpdateRoleCommand
    where TRole : IdentityRole
{
    public async ValueTask HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var role = await roleManager.FindByIdAsync(command.Id.ToString())
                   ?? throw EntityNotFoundException.For<TRole>(command.Id);

        applier.Apply(role, command);
        var result = await roleManager.UpdateAsync(role);

        if (!result.Succeeded)
            throw new IdentityException(result);
    }
}

using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Modules.Identity.Application.Abstractions;
using Cheetah.Modules.Identity.Application.Exceptions;
using Cheetah.Modules.Identity.Domain;
using Cheetah.Modules.Identity.DomainEvents;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Application.Commands;

/// <summary>
/// Обобщённый хендлер создания пользователя. Конкретику (сборку доменной сущности из
/// команды) поставляет хост через <see cref="ICreateUserFactory{TUser,TCommand}"/>.
/// Регистрируется builder-ом под конкретный тип команды хоста.
/// </summary>
public sealed class CreateUserCommandHandler<TUser, TRole, TCommand>(
    UserManager<TUser> userManager,
    RoleManager<TRole> roleManager,
    IEventBus eventBus,
    ICreateUserFactory<TUser, TCommand> factory)
    : ICommandHandler<TCommand, Guid>
    where TCommand : CreateUserCommand
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    public async ValueTask<Guid> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var user = factory.Create(command);
        var result = await userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
            throw new IdentityException(result);

        if (command.RoleIds is { Count: > 0 })
        {
            // Identity намеренно построен на ASP.NET Core Identity (UserManager/RoleManager),
            // а не на Cheetah Repository/Specification. RoleManager.Roles — это штатный API
            // фреймворка, поэтому LINQ-проекция здесь идиоматична и не нарушает правило спецификаций.
            var roleNames = await roleManager.Roles
                .Where(r => command.RoleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync(ct);

            var roleResult = await userManager.AddToRolesAsync(user, roleNames);
            if (!roleResult.Succeeded)
                throw new IdentityException(roleResult);
        }

        await eventBus.PublishAsync(new UserCreatedEvent(user.Id, user.UserName, user.Email), ct);

        return user.Id;
    }
}

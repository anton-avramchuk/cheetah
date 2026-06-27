using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Domain;

namespace Cheetah.Modules.Identity.Application.Abstractions;

// Стратегии, через которые хост подставляет в обобщённые хендлеры свою конкретику:
// как из команды собрать доменную сущность и как применить изменения (включая доп. поля).
// Регистрируются делегатами из AddCrmIdentity(...).WithUsers/WithRoles(...).

/// <summary>Фабрика доменного пользователя из команды создания.</summary>
public interface ICreateUserFactory<out TUser, in TCommand>
    where TCommand : CreateUserCommand
{
    TUser Create(TCommand command);
}

/// <summary>Применение изменений команды обновления к доменному пользователю.</summary>
public interface IUpdateUserApplier<in TUser, in TCommand>
    where TCommand : UpdateUserCommand
{
    void Apply(TUser user, TCommand command);
}

/// <summary>Фабрика доменной роли из команды создания.</summary>
public interface ICreateRoleFactory<out TRole, in TCommand>
    where TCommand : CreateRoleCommand
{
    TRole Create(TCommand command);
}

/// <summary>Применение изменений команды обновления к доменной роли.</summary>
public interface IUpdateRoleApplier<in TRole, in TCommand>
    where TCommand : UpdateRoleCommand
{
    void Apply(TRole role, TCommand command);
}

// --- Делегатные реализации, которыми builder оборачивает лямбды хоста ---

public sealed class DelegateCreateUserFactory<TUser, TCommand>(Func<TCommand, TUser> create)
    : ICreateUserFactory<TUser, TCommand>
    where TCommand : CreateUserCommand
{
    public TUser Create(TCommand command) => create(command);
}

public sealed class DelegateCreateRoleFactory<TRole, TCommand>(Func<TCommand, TRole> create)
    : ICreateRoleFactory<TRole, TCommand>
    where TCommand : CreateRoleCommand
{
    public TRole Create(TCommand command) => create(command);
}

/// <summary>
/// Базовое обновление пользователя (имя/email/security stamp) + опциональная хостовая
/// лямбда для дополнительных полей.
/// </summary>
public sealed class DelegateUpdateUserApplier<TUser, TRole, TCommand>(Action<TUser, TCommand>? extra = null)
    : IUpdateUserApplier<TUser, TCommand>
    where TCommand : UpdateUserCommand
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    public void Apply(TUser user, TCommand command)
    {
        user.ChangeUserName(command.UserName);
        user.ChangeEmail(command.Email);
        user.RefreshSecurityStamp();
        extra?.Invoke(user, command);
    }
}

/// <summary>Базовое обновление роли (имя) + опциональная хостовая лямбда.</summary>
public sealed class DelegateUpdateRoleApplier<TRole, TCommand>(Action<TRole, TCommand>? extra = null)
    : IUpdateRoleApplier<TRole, TCommand>
    where TCommand : UpdateRoleCommand
    where TRole : IdentityRole
{
    public void Apply(TRole role, TCommand command)
    {
        role.ChangeName(command.Name);
        extra?.Invoke(role, command);
    }
}

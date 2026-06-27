using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Modules.Identity.Application.Abstractions;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;
using Cheetah.Modules.Identity.Domain;
using Cheetah.Modules.Identity.Infrastructure.Context;
using Cheetah.Modules.Identity.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using IdentityOptions = Microsoft.AspNetCore.Identity.IdentityOptions;

namespace Cheetah.Modules.Identity.Api.Registration;

/// <summary>
/// Единая точка подключения базового модуля Identity в реальном приложении.
/// Хост задаёт свои доменные типы и расширенные контракты, а builder регистрирует
/// Identity-инфраструктуру, обобщённые CQRS-хендлеры, стратегии сборки сущностей и
/// замыкания регистрации HTTP-эндпоинтов.
/// </summary>
public static class AddCrmIdentityExtensions
{
    public static CrmIdentityBuilder<TUser, TRole, TDbContext> AddCrmIdentity<TUser, TRole, TDbContext>(
        this IServiceCollection services,
        Action<IdentityOptions>? configureIdentity = null)
        where TDbContext : CheetahIdentityDbContext<TDbContext, TUser, TRole>
        where TRole : IdentityRole
        where TUser : IdentityUser<TRole>
    {
        services.AddIdentityContext<TDbContext, TUser, TRole>(configureIdentity ?? (_ => { }));

        // Аутентификация: типы фиксированы, хост их не расширяет.
        services.AddScoped<ICommandHandler<LoginCommand, TokenResult>, LoginCommandHandler<TUser, TRole>>();

        return new CrmIdentityBuilder<TUser, TRole, TDbContext>(services);
    }
}

/// <summary>Fluent-builder для конфигурации срезов Users / Roles модуля Identity.</summary>
public sealed class CrmIdentityBuilder<TUser, TRole, TDbContext>(IServiceCollection services)
    where TDbContext : CheetahIdentityDbContext<TDbContext, TUser, TRole>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    public IServiceCollection Services => services;

    /// <summary>
    /// Подключить срез пользователей. <paramref name="createUser"/> собирает доменную
    /// сущность из команды; <paramref name="applyUserChanges"/> (опционально) применяет
    /// дополнительные поля при обновлении (базовые имя/email/security stamp применяются всегда).
    /// </summary>
    public CrmIdentityBuilder<TUser, TRole, TDbContext> WithUsers<
        TCreateRequest, TCreateCommand,
        TUpdateRequest, TUpdateCommand,
        TUserModel, TUserDetailModel,
        TUserGridVm, TUserDetailVm>(
        Func<TCreateCommand, TUser> createUser,
        Action<TUser, TUpdateCommand>? applyUserChanges = null)
        where TCreateRequest : CreateUserRequest
        where TCreateCommand : CreateUserCommand
        where TUpdateRequest : UpdateUserRequest
        where TUpdateCommand : UpdateUserCommand
        where TUserModel : UserModel
        where TUserDetailModel : UserDetailModel
        where TUserGridVm : class, ICrmResponse
        where TUserDetailVm : class, ICrmResponse
    {
        // Стратегии сборки/обновления сущности
        services.AddSingleton<ICreateUserFactory<TUser, TCreateCommand>>(
            new DelegateCreateUserFactory<TUser, TCreateCommand>(createUser));
        services.AddSingleton<IUpdateUserApplier<TUser, TUpdateCommand>>(
            new DelegateUpdateUserApplier<TUser, TRole, TUpdateCommand>(applyUserChanges));

        // CQRS-хендлеры под конкретные типы хоста
        services.AddScoped<ICommandHandler<TCreateCommand, Guid>, CreateUserCommandHandler<TUser, TRole, TCreateCommand>>();
        services.AddScoped<ICommandHandler<TUpdateCommand>, UpdateUserCommandHandler<TUser, TRole, TUpdateCommand>>();
        services.AddScoped<ICommandHandler<DeleteUserCommand>, DeleteUserCommandHandler<TUser, TRole>>();
        services.AddScoped<IQueryHandler<GetUserByIdQuery<TUserDetailModel>, TUserDetailModel?>, GetUserByIdQueryHandler<TUser, TRole, TUserDetailModel>>();
        services.AddScoped<IQueryHandler<GetUsersGridQuery<TUserModel>, GridResult<TUserModel>>, GetUsersGridQueryHandler<TUser, TRole, TUserModel>>();

        // Регистрация эндпоинтов (выполнится в OnApplicationInitialization)
        services.AddSingleton<IIdentityEndpointRegistrar>(new DelegateEndpointRegistrar(routes =>
            IdentityEndpointMapper.MapUserEndpoints<
                TCreateRequest, TCreateCommand,
                TUpdateRequest, TUpdateCommand,
                TUserModel, TUserDetailModel,
                TUserGridVm, TUserDetailVm>(routes)));

        return this;
    }

    /// <summary>Подключить срез ролей (см. <see cref="WithUsers{T1,T2,T3,T4,T5,T6,T7,T8}"/>).</summary>
    public CrmIdentityBuilder<TUser, TRole, TDbContext> WithRoles<
        TCreateRequest, TCreateCommand,
        TUpdateRequest, TUpdateCommand,
        TRoleModel,
        TRoleGridVm, TRoleVm>(
        Func<TCreateCommand, TRole> createRole,
        Action<TRole, TUpdateCommand>? applyRoleChanges = null)
        where TCreateRequest : CreateRoleRequest
        where TCreateCommand : CreateRoleCommand
        where TUpdateRequest : UpdateRoleRequest
        where TUpdateCommand : UpdateRoleCommand
        where TRoleModel : RoleModel
        where TRoleGridVm : class, ICrmResponse
        where TRoleVm : class, ICrmResponse
    {
        services.AddSingleton<ICreateRoleFactory<TRole, TCreateCommand>>(
            new DelegateCreateRoleFactory<TRole, TCreateCommand>(createRole));
        services.AddSingleton<IUpdateRoleApplier<TRole, TUpdateCommand>>(
            new DelegateUpdateRoleApplier<TRole, TUpdateCommand>(applyRoleChanges));

        services.AddScoped<ICommandHandler<TCreateCommand, Guid>, CreateRoleCommandHandler<TRole, TCreateCommand>>();
        services.AddScoped<ICommandHandler<TUpdateCommand>, UpdateRoleCommandHandler<TRole, TUpdateCommand>>();
        services.AddScoped<ICommandHandler<DeleteRoleCommand>, DeleteRoleCommandHandler<TRole>>();
        services.AddScoped<IQueryHandler<GetRoleByIdQuery<TRoleModel>, TRoleModel?>, GetRoleByIdQueryHandler<TRole, TRoleModel>>();
        services.AddScoped<IQueryHandler<GetRolesGridQuery<TRoleModel>, GridResult<TRoleModel>>, GetRolesGridQueryHandler<TRole, TRoleModel>>();

        services.AddSingleton<IIdentityEndpointRegistrar>(new DelegateEndpointRegistrar(routes =>
            IdentityEndpointMapper.MapRoleEndpoints<
                TCreateRequest, TCreateCommand,
                TUpdateRequest, TUpdateCommand,
                TRoleModel,
                TRoleGridVm, TRoleVm>(routes)));

        return this;
    }
}

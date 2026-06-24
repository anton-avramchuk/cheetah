using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Mapping;

/// <summary>
/// Реестр генерируемых мапперов Identity (аналог Mapster-профиля, но через source generator).
///
/// Ключевое отличие от <see cref="MapFromAttribute"/>: маркер <see cref="GenerateMapperAttribute"/>
/// висит не на типе-приёмнике (Request/ViewModel в Contracts), а здесь, в выделенной маппинг-сборке.
/// Поэтому сгенерированные extension-методы (<c>source.MapTo*()</c> / <c>query.ProjectTo*()</c>)
/// осядут в неймспейсе <c>Cheetah.Modules.Identity.Mapping</c>, а сборка Contracts остаётся чистой
/// (зависит только от Core + Shared, без ссылки на Application/Domain).
///
/// Переименования покрываются <see cref="MapMemberAttribute"/>, константы — <see cref="MapConstantAttribute"/>.
/// Открытые генерики (Grid-запросы вида <c>GetRolesGridQuery&lt;&gt;</c>) пока остаются за Mapster.
/// </summary>
// Auth
[GenerateMapper(typeof(LoginRequest), typeof(LoginCommand), GenerateProjection = false)]
[GenerateMapper(typeof(TokenResult), typeof(TokenViewModel), GenerateProjection = false)]
[MapMember(typeof(TokenViewModel), nameof(TokenViewModel.AccessToken), nameof(TokenResult.Token))]
[MapConstant(typeof(TokenViewModel), nameof(TokenViewModel.TokenType), "Bearer")]
[MapMember(typeof(TokenViewModel), nameof(TokenViewModel.ExpiresIn), nameof(TokenResult.ExpiresInSeconds))]
// Roles
[GenerateMapper(typeof(RoleModel), typeof(RoleViewModel))] // read-проекция → ProjectTo нужен
[GenerateMapper(typeof(GetRoleByIdRequest), typeof(GetRoleByIdQuery), GenerateProjection = false)]
[GenerateMapper(typeof(CreateRoleRequest), typeof(CreateRoleCommand), GenerateProjection = false)]
[GenerateMapper(typeof(UpdateRoleRequest), typeof(UpdateRoleCommand), GenerateProjection = false)]
[GenerateMapper(typeof(DeleteRoleRequest), typeof(DeleteRoleCommand), GenerateProjection = false)]
// Users
[GenerateMapper(typeof(GetUserByIdRequest), typeof(GetUserByIdQuery), GenerateProjection = false)]
[GenerateMapper(typeof(CreateUserRequest), typeof(CreateUserCommand), GenerateProjection = false)]
[GenerateMapper(typeof(UpdateUserRequest), typeof(UpdateUserCommand), GenerateProjection = false)]
[GenerateMapper(typeof(DeleteUserRequest), typeof(DeleteUserCommand), GenerateProjection = false)]
public static partial class IdentityMappingRegistry
{
}

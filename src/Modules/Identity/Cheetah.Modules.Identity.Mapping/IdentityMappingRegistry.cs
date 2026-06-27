using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Mapping;

/// <summary>
/// Реестр генерируемых мапперов Identity (аналог Mapster-профиля, но через source generator).
///
/// С абстрактизацией контрактов (Identity — базовый модуль) генерируемые мапперы остаются
/// только для аутентификации: Request/Command/ViewModel пользователей и ролей теперь
/// абстрактны, поэтому их маппинг — ответственность конкретных типов хоста (по конвенции).
/// </summary>
// Auth
[GenerateMapper(typeof(LoginRequest), typeof(LoginCommand), GenerateProjection = false)]
[GenerateMapper(typeof(TokenResult), typeof(TokenViewModel), GenerateProjection = false)]
[MapMember(typeof(TokenViewModel), nameof(TokenViewModel.AccessToken), nameof(TokenResult.Token))]
[MapConstant(typeof(TokenViewModel), nameof(TokenViewModel.TokenType), "Bearer")]
[MapMember(typeof(TokenViewModel), nameof(TokenViewModel.ExpiresIn), nameof(TokenResult.ExpiresInSeconds))]
public static partial class IdentityMappingRegistry
{
}

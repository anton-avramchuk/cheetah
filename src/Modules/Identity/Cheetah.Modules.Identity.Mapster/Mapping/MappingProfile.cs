using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;
using Mapster;

namespace Cheetah.Modules.Identity.Mapster.Mapping;

/// <summary>
/// Mapster-профиль Identity. С абстрактизацией контрактов (Identity — базовый модуль)
/// маппинги Request→Command и Model→ViewModel выполняются по конвенции для конкретных
/// типов хоста, поэтому здесь остаётся только аутентификация с нестандартными правилами.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<LoginRequest, LoginCommand>();
        config.NewConfig<TokenResult, TokenViewModel>()
            .Map(dest => dest.AccessToken, src => src.Token)
            .Map(dest => dest.TokenType, _ => "Bearer")
            .Map(dest => dest.ExpiresIn, src => src.ExpiresInSeconds);
    }
}

using Cheetah.Mapping.Core;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Mapping.Mapster.DependencyInjection;

public static class RegistrationExtensions
{
    /// <summary>
    /// Профиль — это набор правил, а не состояние запроса, поэтому он синглтон: маппер собирается
    /// один раз на приложение и обязан уметь достать профили из корневого провайдера.
    /// </summary>
    public static IServiceCollection AddMapping<TMapping>(this IServiceCollection services) where TMapping : class, IMapsterMappingProfile
    {
        services.AddSingleton<IMapsterMappingProfile, TMapping>();
        return services;
    }

    /// <summary>
    /// Маппер и его конфигурация — одни на приложение.
    ///
    /// Раньше и то и другое было scoped: фабрика прогоняла все <see cref="IMapsterMappingProfile"/>
    /// поверх общего конфига на КАЖДЫЙ HTTP-запрос. Нагрузочный прогон показал, во что это
    /// обходится: пропускная способность падала на глазах (179 → 98 → 78 rps за три прогона
    /// подряд), память процесса росла вместе с раздувающимся конфигом, а процессор уходил целиком
    /// на перерегистрацию правил — при полностью простаивающих базе и соседних сервисах. Хуже
    /// того, <c>NewConfig</c> поверх уже скомпилированного конфига — это гонка: параллельные
    /// запросы на непрогретом приложении отвечали 500 с
    /// <c>TypeAdapter.Adapt was already called</c>.
    ///
    /// Конфиг — собственный экземпляр, а не <see cref="TypeAdapterConfig.GlobalSettings"/>:
    /// глобальный общий на процесс, поэтому два приложения в одном процессе (так поднимаются
    /// тесты) конфигурировали бы его по очереди и ловили ту же гонку.
    /// </summary>
    public static IServiceCollection AddCheetahMapster(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(_ => new TypeAdapterConfig());
        services.TryAddSingleton<IObjectMapper>(provider =>
        {
            var config = provider.GetRequiredService<TypeAdapterConfig>();

            foreach (var profile in provider.GetServices<IMapsterMappingProfile>())
                profile.Configure(config);

            return new MapsterObjectMapper(config);
        });

        return services;
    }
}

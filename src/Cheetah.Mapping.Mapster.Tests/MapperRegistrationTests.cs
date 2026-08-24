using Cheetah.Mapping.Core;
using Cheetah.Mapping.Mapster.DependencyInjection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cheetah.Mapping.Mapster.Tests;

/// <summary>
/// Правила регистрации маппера. Всё, что здесь проверяется, обнаружилось нагрузочным прогоном:
/// конфигурация профилей на каждый запрос съедала процессор целиком, раздувала конфиг и роняла
/// параллельные запросы на непрогретом процессе.
/// </summary>
public sealed class MapperRegistrationTests
{
    private sealed record Source(string Name);

    private sealed record Destination(string Name);

    /// <summary>Считает, сколько раз к нему пришли за правилами.</summary>
    private sealed class CountingProfile : IMapsterMappingProfile
    {
        private int _calls;

        public int Calls => _calls;

        public void Configure(TypeAdapterConfig config)
        {
            Interlocked.Increment(ref _calls);
            config.NewConfig<Source, Destination>().MapWith(src => new Destination(src.Name));
        }
    }

    private static ServiceProvider BuildProvider(CountingProfile profile)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMapsterMappingProfile>(profile);
        services.AddCheetahMapster();

        // Как в приложении: проверка областей включена, поэтому синглтон не может зависеть от scoped.
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });
    }

    [Fact]
    public void Профили_настраиваются_один_раз_на_приложение_а_не_на_запрос()
    {
        var profile = new CountingProfile();
        using var provider = BuildProvider(profile);

        for (var i = 0; i < 50; i++)
        {
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IObjectMapper>()
                .Map<Source, Destination>(new Source("Иванов"))
                .Name.ShouldBe("Иванов");
        }

        profile.Calls.ShouldBe(1);
    }

    [Fact]
    public async Task Параллельные_запросы_на_непрогретом_приложении_не_падают()
    {
        var profile = new CountingProfile();
        using var provider = BuildProvider(profile);

        var tasks = Enumerable.Range(0, 64).Select(i => Task.Run(() =>
        {
            using var scope = provider.CreateScope();
            var mapper = scope.ServiceProvider.GetRequiredService<IObjectMapper>();
            return mapper.Map<Source, Destination>(new Source($"Кандидат {i}")).Name;
        }));

        var names = await Task.WhenAll(tasks);

        names.ShouldBeUnique();
        profile.Calls.ShouldBe(1);
    }

    [Fact]
    public void Маппер_один_на_приложение()
    {
        using var provider = BuildProvider(new CountingProfile());

        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        first.ServiceProvider.GetRequiredService<IObjectMapper>()
            .ShouldBeSameAs(second.ServiceProvider.GetRequiredService<IObjectMapper>());
    }
}

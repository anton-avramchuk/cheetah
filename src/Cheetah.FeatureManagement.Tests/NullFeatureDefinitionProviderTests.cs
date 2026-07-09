using Shouldly;

namespace Cheetah.FeatureManagement.Tests;

/// <summary>
/// Регресс: хост, транзитивно зависящий от <see cref="CrmFeatureManagementModule"/> (например, через
/// <c>RequireFeature</c> в Cheetah.Backend.Endpoints), но не подключающий ни Infrastructure, ни Client,
/// не должен падать при валидации DI — <see cref="IFeatureManager"/> должен резолвиться с безопасным
/// провайдером-заглушкой (все флаги — выключены).
/// </summary>
public class NullFeatureDefinitionProviderTests
{
    [Fact]
    public async Task GetAsync_returns_null_for_any_key()
    {
        var provider = new NullFeatureDefinitionProvider();

        (await provider.GetAsync("any.key", null)).ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsync_returns_empty()
    {
        var provider = new NullFeatureDefinitionProvider();

        (await provider.GetAllAsync(null)).ShouldBeEmpty();
    }

    [Fact]
    public async Task FeatureManager_with_null_provider_reports_everything_disabled()
    {
        var manager = new FeatureManager(new NullFeatureDefinitionProvider(), []);

        (await manager.IsEnabledAsync("Vacancy.Teams")).ShouldBeFalse();
    }
}

using System.Security.Claims;
using Cheetah.FeatureManagement;
using Cheetah.Permissions;
using Shouldly;

namespace Cheetah.Permissions.Tests;

public class PermissionAuthorizerTests
{
    private static ClaimsPrincipal User(params string[] permissions)
    {
        var claims = permissions.Select(p => new Claim(PermissionConstants.PermissionClaimType, p));
        var identity = new ClaimsIdentity(claims, authenticationType: "test");
        return new ClaimsPrincipal(identity);
    }

    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    /// <summary>Авторизатор без привязок к фичам — поведение «как раньше», по одним claims.</summary>
    private static IPermissionAuthorizer ClaimsOnly() => Build(registry => { }, _ => true);

    private static IPermissionAuthorizer Build(Action<PermissionRegistry> declare, Func<string, bool> featureEnabled)
    {
        var registry = new PermissionRegistry();
        declare(registry);
        return new ClaimPermissionAuthorizer(registry, new StubFeatureManager(featureEnabled));
    }

    // ---- Claims ------------------------------------------------------------

    [Fact]
    public async Task Has_Returns_True_When_Claim_Present()
        => (await ClaimsOnly().HasAsync(User("Documents.Sign"), "Documents.Sign")).ShouldBeTrue();

    [Fact]
    public async Task Has_Returns_False_When_Claim_Absent()
        => (await ClaimsOnly().HasAsync(User("Documents.Read"), "Documents.Sign")).ShouldBeFalse();

    [Fact]
    public async Task Has_Returns_False_For_Anonymous_User()
        => (await ClaimsOnly().HasAsync(Anonymous, "Documents.Sign")).ShouldBeFalse();

    [Fact]
    public async Task HasAll_True_Only_When_All_Present()
    {
        var sut = ClaimsOnly();
        var u = User("A", "B", "C");
        (await sut.HasAllAsync(u, ["A", "B"])).ShouldBeTrue();
        (await sut.HasAllAsync(u, ["A", "B", "D"])).ShouldBeFalse();
    }

    [Fact]
    public async Task HasAny_True_When_At_Least_One_Present()
    {
        var sut = ClaimsOnly();
        var u = User("A");
        (await sut.HasAnyAsync(u, ["A", "B"])).ShouldBeTrue();
        (await sut.HasAnyAsync(u, ["B", "C"])).ShouldBeFalse();
    }

    // ---- Фич-флаги ---------------------------------------------------------

    [Fact]
    public async Task Disabled_feature_revokes_permission_even_with_claim()
    {
        var sut = Build(r => r.Add("Vacancy.Teams.View", "", "Vacancy", feature: "Vacancy.Teams"),
            featureEnabled: _ => false);

        (await sut.HasAsync(User("Vacancy.Teams.View"), "Vacancy.Teams.View")).ShouldBeFalse();
    }

    [Fact]
    public async Task Enabled_feature_keeps_permission()
    {
        var sut = Build(r => r.Add("Vacancy.Teams.View", "", "Vacancy", feature: "Vacancy.Teams"),
            featureEnabled: f => f == "Vacancy.Teams");

        (await sut.HasAsync(User("Vacancy.Teams.View"), "Vacancy.Teams.View")).ShouldBeTrue();
    }

    /// <summary>Реестр знает только permissions этого процесса. Чужой гейтить нечем — судим по claims.</summary>
    [Fact]
    public async Task Permission_unknown_to_registry_falls_back_to_claims()
    {
        var sut = Build(r => { }, featureEnabled: _ => false);

        (await sut.HasAsync(User("Foreign.Service.Do"), "Foreign.Service.Do")).ShouldBeTrue();
    }

    [Fact]
    public async Task Permission_without_feature_is_not_affected_by_flags()
    {
        var sut = Build(r => r.Add("Documents.Sign", "", "Documents"), featureEnabled: _ => false);

        (await sut.HasAsync(User("Documents.Sign"), "Documents.Sign")).ShouldBeTrue();
    }

    [Fact]
    public async Task HasAll_False_When_One_Permissions_Feature_Is_Off()
    {
        var sut = Build(
            r =>
            {
                r.Add("A", "", "M");
                r.Add("B", "", "M", feature: "Off");
            },
            featureEnabled: f => f != "Off");

        (await sut.HasAllAsync(User("A", "B"), ["A", "B"])).ShouldBeFalse();
    }

    [Fact]
    public async Task HasAny_Ignores_Permissions_Whose_Feature_Is_Off()
    {
        var sut = Build(r => r.Add("B", "", "M", feature: "Off"), featureEnabled: f => f != "Off");

        (await sut.HasAnyAsync(User("B"), ["B"])).ShouldBeFalse();
    }

    /// <summary>Фичу спрашиваем только у permission'ов, которые к ней привязаны.</summary>
    [Fact]
    public async Task Feature_is_not_queried_for_unbound_permissions()
    {
        var asked = new List<string>();
        var registry = new PermissionRegistry();
        registry.Add("Documents.Sign", "", "Documents");
        var sut = new ClaimPermissionAuthorizer(registry, new StubFeatureManager(_ => true, asked));

        await sut.HasAsync(User("Documents.Sign"), "Documents.Sign");

        asked.ShouldBeEmpty();
    }

    internal sealed class StubFeatureManager(Func<string, bool> enabled, List<string>? asked = null) : IFeatureManager
    {
        public ValueTask<bool> IsEnabledAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default)
        {
            asked?.Add(featureKey);
            return ValueTask.FromResult(enabled(featureKey));
        }

        public ValueTask<FeatureVariant?> GetVariantAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default)
            => ValueTask.FromResult<FeatureVariant?>(null);
    }
}

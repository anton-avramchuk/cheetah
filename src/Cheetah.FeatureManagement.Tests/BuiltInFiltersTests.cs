using Cheetah.FeatureManagement.Filters;
using Shouldly;

namespace Cheetah.FeatureManagement.Tests;

public class BuiltInFiltersTests
{
    private static FeatureFilterContext Ctx(FeatureContext subject, Dictionary<string, object?> p)
        => new("k", subject, p);

    [Fact]
    public async Task Users_filter_matches_listed_user()
    {
        var user = Guid.NewGuid();
        var f = new UsersFeatureFilter();

        (await f.EvaluateAsync(Ctx(new FeatureContext { UserId = user },
            new() { ["users"] = new[] { user.ToString() } }), default)).ShouldBeTrue();

        (await f.EvaluateAsync(Ctx(new FeatureContext { UserId = Guid.NewGuid() },
            new() { ["users"] = new[] { user.ToString() } }), default)).ShouldBeFalse();
    }

    [Fact]
    public async Task Roles_filter_matches_on_intersection()
    {
        var f = new RolesFeatureFilter();
        var subject = new FeatureContext { Roles = new[] { "admin", "sales" } };

        (await f.EvaluateAsync(Ctx(subject, new() { ["roles"] = new[] { "sales" } }), default)).ShouldBeTrue();
        (await f.EvaluateAsync(Ctx(subject, new() { ["roles"] = new[] { "manager" } }), default)).ShouldBeFalse();
    }

    [Fact]
    public async Task Tenants_filter_matches_listed_tenant()
    {
        var tenant = Guid.NewGuid();
        var f = new TenantsFeatureFilter();

        (await f.EvaluateAsync(Ctx(new FeatureContext { TenantId = tenant },
            new() { ["tenants"] = new[] { tenant.ToString() } }), default)).ShouldBeTrue();
    }

    [Fact]
    public async Task TimeWindow_filter_respects_bounds()
    {
        var f = new TimeWindowFeatureFilter();
        var now = DateTimeOffset.UtcNow;

        (await f.EvaluateAsync(Ctx(FeatureContext.Empty, new()
        {
            ["from"] = now.AddHours(-1).ToString("O"),
            ["to"] = now.AddHours(1).ToString("O")
        }), default)).ShouldBeTrue();

        (await f.EvaluateAsync(Ctx(FeatureContext.Empty, new()
        {
            ["from"] = now.AddHours(1).ToString("O")
        }), default)).ShouldBeFalse();
    }
}

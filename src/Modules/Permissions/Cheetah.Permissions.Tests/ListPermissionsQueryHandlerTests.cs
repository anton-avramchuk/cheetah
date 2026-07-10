using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.FeatureManagement;
using Cheetah.Permissions.Catalog.Application;
using Cheetah.Permissions.Catalog.Domain;
using Shouldly;

namespace Cheetah.Permissions.Tests;

/// <summary>
/// Permission можно привязать к фиче. Выключенная (или вовсе незаведённая) фича прячет свои
/// permissions из каталога — админ не должен видеть право на функциональность, которой нет.
/// </summary>
public class ListPermissionsQueryHandlerTests
{
    private static PermissionDefinition Def(string key, string? feature = null)
        => PermissionDefinition.Create(key, description: key, module: "Test", feature: feature);

    private static async Task<IReadOnlyList<PermissionDefinitionDto>> ListAsync(
        IEnumerable<PermissionDefinition> stored, Func<string, bool> featureEnabled, List<string>? asked = null)
    {
        var handler = new ListPermissionsQueryHandler(
            new FakeRepository(stored.ToList()),
            new FakeFeatureManager(featureEnabled, asked ?? []));

        return await handler.HandleAsync(new ListPermissionsQuery());
    }

    [Fact]
    public async Task Permission_without_feature_is_always_listed()
    {
        var result = await ListAsync([Def("Clients.View")], _ => false);

        result.Select(p => p.Key).ShouldBe(["Clients.View"]);
    }

    [Fact]
    public async Task Permission_of_enabled_feature_is_listed()
    {
        var result = await ListAsync([Def("Vacancy.Teams.View", feature: "Vacancy.Teams")], _ => true);

        result.Select(p => p.Key).ShouldBe(["Vacancy.Teams.View"]);
        result.Single().Feature.ShouldBe("Vacancy.Teams");
    }

    [Fact]
    public async Task Permission_of_disabled_feature_is_hidden()
    {
        var result = await ListAsync([Def("Vacancy.Teams.View", feature: "Vacancy.Teams")], _ => false);

        result.ShouldBeEmpty();
    }

    /// <summary>Незаведённый флаг IFeatureManager трактует как выключенный — permission прячется.</summary>
    [Fact]
    public async Task Permission_of_unknown_feature_is_hidden()
    {
        var result = await ListAsync(
            [Def("Ghost.View", feature: "Never.Registered")],
            feature => feature == "Some.Other.Feature");

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Feature_state_is_asked_once_per_distinct_feature()
    {
        var asked = new List<string>();

        var result = await ListAsync(
            [Def("A.View", "F1"), Def("B.View", "F1"), Def("C.View", "F2"), Def("D.View")],
            feature => feature == "F1",
            asked);

        // F1 включена → A и B; F2 выключена → C скрыт; D без фичи → виден всегда.
        result.Select(p => p.Key).ShouldBe(["A.View", "B.View", "D.View"]);
        asked.Order().ShouldBe(["F1", "F2"]);
    }

    private sealed class FakeFeatureManager(Func<string, bool> enabled, List<string> asked) : IFeatureManager
    {
        public ValueTask<bool> IsEnabledAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default)
        {
            asked.Add(featureKey);
            return ValueTask.FromResult(enabled(featureKey));
        }

        public ValueTask<FeatureVariant?> GetVariantAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default)
            => ValueTask.FromResult<FeatureVariant?>(null);
    }

    internal sealed class FakeRepository(List<PermissionDefinition> items) : IRepository<PermissionDefinition, string>
    {
        public List<PermissionDefinition> Items => items;

        public ValueTask<List<PermissionDefinition>> GetAllAsync(
            ISpecification<PermissionDefinition>? spec = null, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(spec is null ? items : items.Where(spec.ToExpression().Compile()).ToList());

        public ValueTask<PermissionDefinition?> GetByIdAsync(string id, CancellationToken ct = default)
            => ValueTask.FromResult(items.FirstOrDefault(x => x.Id == id));

        public ValueTask<PermissionDefinition?> GetBySpecAsync(ISpecification<PermissionDefinition> spec, CancellationToken ct = default)
            => ValueTask.FromResult(items.FirstOrDefault(spec.ToExpression().Compile()));

        public ValueTask<bool> ExistsAsync(ISpecification<PermissionDefinition> spec, CancellationToken ct = default)
            => ValueTask.FromResult(items.Any(spec.ToExpression().Compile()));

        public IQueryable<PermissionDefinition> AsQueryable() => items.AsQueryable();
        public IQueryable<PermissionDefinition> AsNoTrackingQueryable() => items.AsQueryable();

        public void Add(PermissionDefinition entity) => items.Add(entity);
        public void Update(PermissionDefinition entity) { }
        public void Delete(PermissionDefinition entity) => items.Remove(entity);
        public ValueTask<int> SaveChangesAsync(CancellationToken ct = default) => ValueTask.FromResult(0);
    }
}

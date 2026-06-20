using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Application.Commands;
using Cheetah.Modules.FeatureManagement.Application.Queries;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.DomainEvents;
using Shouldly;

namespace Cheetah.Modules.FeatureManagement.Application.Tests;

public class HandlerTests
{
    [Fact]
    public async Task Enable_publishes_toggled_event()
    {
        var flag = TestFlag.Create("deals.kanban-v2", "Kanban v2", "deals");
        var repo = new InMemoryFlagRepository(flag);
        var bus = new RecordingEventBus();
        var handler = new EnableFeatureFlagCommandHandler<TestFlag>(repo, bus);

        await handler.HandleAsync(new EnableFeatureFlagCommand("deals.kanban-v2"));

        flag.Enabled.ShouldBeTrue();
        repo.SaveCount.ShouldBe(1);
        bus.Published.OfType<FeatureFlagToggledIntegrationEvent>().Single().Enabled.ShouldBeTrue();
    }

    [Fact]
    public async Task SetTargeting_replaces_rules_and_publishes_changed()
    {
        var flag = TestFlag.Create("deals.kanban-v2", "Kanban v2", "deals");
        var repo = new InMemoryFlagRepository(flag);
        var bus = new RecordingEventBus();
        var handler = new SetTargetingCommandHandler<TestFlag>(repo, bus);

        var rules = new[]
        {
            new TargetingRuleDto(0, "Percentage", new Dictionary<string, object?> { ["percentage"] = 25 })
        };
        await handler.HandleAsync(new SetTargetingCommand("deals.kanban-v2", rules));

        flag.Rules.ShouldHaveSingleItem();
        flag.Rules[0].FilterName.ShouldBe("Percentage");
        flag.Rules[0].ParametersJson.ShouldContain("percentage");
        bus.Published.OfType<FeatureFlagChangedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Sync_creates_new_and_preserves_existing_settings()
    {
        // существующий флаг включён и с таргетингом
        var existing = TestFlag.Create("deals.kanban-v2", "Kanban v2", "deals");
        existing.Enable();
        var repo = new InMemoryFlagRepository(existing);
        var bus = new RecordingEventBus();
        var handler = new SyncFeatureRegistryCommandHandler<TestFlag, TestCreateRequest>(repo, new TestFactory(), bus);

        await handler.HandleAsync(new SyncFeatureRegistryCommand(new[]
        {
            new FeatureDefinitionDescriptor("deals.kanban-v2", "Kanban v2 (renamed)", "deals"), // существующий
            new FeatureDefinitionDescriptor("pricing.experiment", "A/B", "pricing", FeatureValueType.Variant) // новый
        }));

        // существующий: метаданные обновлены, Enabled НЕ сброшен, событий о нём нет
        existing.Name.ShouldBe("Kanban v2 (renamed)");
        existing.Enabled.ShouldBeTrue();

        // новый: создан + Created-событие
        var created = await repo.GetByKeyAsync("pricing.experiment", false);
        created.ShouldNotBeNull();
        created!.Enabled.ShouldBeFalse();
        bus.Published.OfType<FeatureFlagCreatedIntegrationEvent>()
            .ShouldHaveSingleItem().Key.ShouldBe("pricing.experiment");
    }

    [Fact]
    public async Task GetByKey_projects_to_dto()
    {
        var flag = TestFlag.Create("deals.kanban-v2", "Kanban v2", "deals");
        var repo = new InMemoryFlagRepository(flag);
        var handler = new GetFeatureFlagByKeyQueryHandler<TestFlag, TestDto>(repo, new TestProjector());

        var dto = await handler.HandleAsync(new GetFeatureFlagByKeyQuery<TestDto>("deals.kanban-v2"));

        dto.ShouldNotBeNull();
        dto!.Key.ShouldBe("deals.kanban-v2");
    }

    [Fact]
    public async Task Evaluate_batches_and_dedupes_keys()
    {
        var def = new FeatureDefinition("a", Enabled: true, FeatureValueType.Bool, [], []);
        var manager = new FeatureManager(new FixedProvider(def), []);
        var handler = new EvaluateFeaturesQueryHandler(manager);

        var result = await handler.HandleAsync(
            new EvaluateFeaturesQuery(new[] { "a", "a", "b" }, FeatureContext.Empty));

        result.Count.ShouldBe(2);          // дедуп
        result["a"].Enabled.ShouldBeTrue();
        result["b"].Enabled.ShouldBeFalse(); // незарегистрированный → выкл
    }

    private sealed class FixedProvider(FeatureDefinition def) : IFeatureDefinitionProvider
    {
        public ValueTask<FeatureDefinition?> GetAsync(string key, Guid? tenantId, CancellationToken ct = default)
            => ValueTask.FromResult(key == def.Key ? def : null);
        public ValueTask<IReadOnlyList<FeatureDefinition>> GetAllAsync(Guid? tenantId, CancellationToken ct = default)
            => ValueTask.FromResult<IReadOnlyList<FeatureDefinition>>([def]);
    }
}

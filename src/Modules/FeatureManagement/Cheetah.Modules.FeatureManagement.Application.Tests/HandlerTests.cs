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
    public async Task Sync_applies_declared_parent_to_new_flag()
    {
        var repo = new InMemoryFlagRepository();
        var handler = new SyncFeatureRegistryCommandHandler<TestFlag, TestCreateRequest>(
            repo, new TestFactory(), new RecordingEventBus());

        await handler.HandleAsync(new SyncFeatureRegistryCommand(
        [
            new FeatureDefinitionDescriptor("Vacancy.Teams", "Командный режим", "Vacancy", ParentKey: "Teams")
        ]));

        (await repo.GetByKeyAsync("Vacancy.Teams", false))!.ParentKey.ShouldBe("Teams");
    }

    [Fact]
    public async Task Sync_applies_declared_parent_to_existing_flag_and_publishes_changed()
    {
        var existing = TestFlag.Create("Vacancy.Teams", "Командный режим", "Vacancy");
        existing.Enable();
        var repo = new InMemoryFlagRepository(existing);
        var bus = new RecordingEventBus();
        var handler = new SyncFeatureRegistryCommandHandler<TestFlag, TestCreateRequest>(repo, new TestFactory(), bus);

        await handler.HandleAsync(new SyncFeatureRegistryCommand(
        [
            new FeatureDefinitionDescriptor("Vacancy.Teams", "Командный режим", "Vacancy", ParentKey: "Teams")
        ]));

        existing.ParentKey.ShouldBe("Teams");
        existing.Enabled.ShouldBeTrue(); // kill-switch по-прежнему не трогаем
        // Меняется эффективное значение флага — реплики потребителей обязаны инвалидироваться.
        bus.Published.OfType<FeatureFlagChangedIntegrationEvent>().ShouldHaveSingleItem().Key.ShouldBe("Vacancy.Teams");
    }

    /// <summary>
    /// Дескриптор без родителя не должен сбрасывать связь, выставленную админом руками:
    /// код декларирует родителя, только когда явно его объявил.
    /// </summary>
    [Fact]
    public async Task Sync_without_declared_parent_keeps_manual_parent()
    {
        var existing = TestFlag.Create("Vacancy.Teams", "Командный режим", "Vacancy");
        existing.SetParent("Teams");
        existing.ClearDomainEvents();
        var repo = new InMemoryFlagRepository(existing);
        var bus = new RecordingEventBus();
        var handler = new SyncFeatureRegistryCommandHandler<TestFlag, TestCreateRequest>(repo, new TestFactory(), bus);

        await handler.HandleAsync(new SyncFeatureRegistryCommand(
        [
            new FeatureDefinitionDescriptor("Vacancy.Teams", "Командный режим", "Vacancy")
        ]));

        existing.ParentKey.ShouldBe("Teams");
        bus.Published.ShouldBeEmpty();
    }

    [Fact]
    public async Task Sync_is_idempotent_for_already_declared_parent()
    {
        var existing = TestFlag.Create("Vacancy.Teams", "Командный режим", "Vacancy");
        existing.SetParent("Teams");
        existing.ClearDomainEvents();
        var repo = new InMemoryFlagRepository(existing);
        var bus = new RecordingEventBus();
        var handler = new SyncFeatureRegistryCommandHandler<TestFlag, TestCreateRequest>(repo, new TestFactory(), bus);

        await handler.HandleAsync(new SyncFeatureRegistryCommand(
        [
            new FeatureDefinitionDescriptor("Vacancy.Teams", "Командный режим", "Vacancy", ParentKey: "Teams")
        ]));

        // Ничего не изменилось — событий нет, реплики не дёргаем на каждом рестарте сервиса.
        bus.Published.ShouldBeEmpty();
    }

    [Fact]
    public async Task SetParent_assigns_and_publishes_changed()
    {
        var parent = TestFlag.Create("Vacancy", "Vacancy", "vacancy");
        var child = TestFlag.Create("Vacancy.Teams", "Teams", "vacancy");
        var repo = new InMemoryFlagRepository(parent, child);
        var bus = new RecordingEventBus();
        var handler = new SetParentFeatureFlagCommandHandler<TestFlag>(repo, bus);

        await handler.HandleAsync(new SetParentFeatureFlagCommand("Vacancy.Teams", "Vacancy"));

        child.ParentKey.ShouldBe("Vacancy");
        bus.Published.OfType<FeatureFlagChangedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public async Task SetParent_missing_parent_throws_not_found()
    {
        var child = TestFlag.Create("Vacancy.Teams", "Teams", "vacancy");
        var repo = new InMemoryFlagRepository(child);
        var handler = new SetParentFeatureFlagCommandHandler<TestFlag>(repo, new RecordingEventBus());

        await Should.ThrowAsync<Cheetah.Core.Domain.Exceptions.EntityNotFoundException>(
            () => handler.HandleAsync(new SetParentFeatureFlagCommand("Vacancy.Teams", "does-not-exist")).AsTask());
    }

    [Fact]
    public async Task SetParent_direct_cycle_is_rejected()
    {
        // A.ParentKey = B, теперь пытаемся сделать B.ParentKey = A — прямой цикл.
        var a = TestFlag.Create("A", "A", "svc");
        var b = TestFlag.Create("B", "B", "svc");
        var repo = new InMemoryFlagRepository(a, b);
        await new SetParentFeatureFlagCommandHandler<TestFlag>(repo, new RecordingEventBus())
            .HandleAsync(new SetParentFeatureFlagCommand("A", "B"));

        var handler = new SetParentFeatureFlagCommandHandler<TestFlag>(repo, new RecordingEventBus());
        await Should.ThrowAsync<InvalidOperationException>(
            () => handler.HandleAsync(new SetParentFeatureFlagCommand("B", "A")).AsTask());
    }

    [Fact]
    public async Task SetParent_indirect_cycle_through_chain_is_rejected()
    {
        // A -> B -> C, пытаемся сделать A родителем C (C.Parent = A) — замкнёт цикл A<-B<-C<-A.
        var a = TestFlag.Create("A", "A", "svc");
        var b = TestFlag.Create("B", "B", "svc");
        var c = TestFlag.Create("C", "C", "svc");
        var repo = new InMemoryFlagRepository(a, b, c);
        var bus = new RecordingEventBus();
        await new SetParentFeatureFlagCommandHandler<TestFlag>(repo, bus)
            .HandleAsync(new SetParentFeatureFlagCommand("A", "B"));
        await new SetParentFeatureFlagCommandHandler<TestFlag>(repo, bus)
            .HandleAsync(new SetParentFeatureFlagCommand("B", "C"));

        var handler = new SetParentFeatureFlagCommandHandler<TestFlag>(repo, bus);
        await Should.ThrowAsync<InvalidOperationException>(
            () => handler.HandleAsync(new SetParentFeatureFlagCommand("C", "A")).AsTask());
    }

    [Fact]
    public async Task SetParent_null_clears_parent()
    {
        var parent = TestFlag.Create("Vacancy", "Vacancy", "vacancy");
        var child = TestFlag.Create("Vacancy.Teams", "Teams", "vacancy");
        child.SetParent("Vacancy");
        var repo = new InMemoryFlagRepository(parent, child);
        var handler = new SetParentFeatureFlagCommandHandler<TestFlag>(repo, new RecordingEventBus());

        await handler.HandleAsync(new SetParentFeatureFlagCommand("Vacancy.Teams", null));

        child.ParentKey.ShouldBeNull();
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

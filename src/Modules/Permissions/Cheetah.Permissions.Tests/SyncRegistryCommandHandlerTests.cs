using Cheetah.Permissions.Catalog.Application;
using Cheetah.Permissions.Catalog.Domain;
using Shouldly;

using FakeRepository = Cheetah.Permissions.Tests.ListPermissionsQueryHandlerTests.FakeRepository;

namespace Cheetah.Permissions.Tests;

/// <summary>
/// Привязка permission к фиче приезжает из сервиса-владельца по сети (RegistrySyncRequest) и обязана
/// доехать до каталога — иначе permission «отвяжется» от фичи и будет виден всегда.
/// </summary>
public class SyncRegistryCommandHandlerTests
{
    private static async Task<FakeRepository> SyncAsync(
        IEnumerable<PermissionDefinition> stored, params PermissionDefinitionDto[] items)
    {
        var repository = new FakeRepository(stored.ToList());
        await new SyncRegistryCommandHandler(repository).HandleAsync(new SyncRegistryCommand("Test", items));
        return repository;
    }

    [Fact]
    public async Task Create_persists_feature()
    {
        var repo = await SyncAsync([], new PermissionDefinitionDto("A.View", "A", "Test", "Some.Feature"));

        repo.Items.Single().Feature.ShouldBe("Some.Feature");
    }

    [Fact]
    public async Task Update_persists_feature()
    {
        var existing = PermissionDefinition.Create("A.View", "A", "Test");

        var repo = await SyncAsync([existing], new PermissionDefinitionDto("A.View", "A", "Test", "Some.Feature"));

        repo.Items.Single().Feature.ShouldBe("Some.Feature");
    }

    [Fact]
    public async Task Update_can_unbind_permission_from_feature()
    {
        var existing = PermissionDefinition.Create("A.View", "A", "Test", "Some.Feature");

        var repo = await SyncAsync([existing], new PermissionDefinitionDto("A.View", "A", "Test", Feature: null));

        repo.Items.Single().Feature.ShouldBeNull();
    }

    /// <summary>Пустая строка — это «фичи нет», а не фича с пустым ключом (иначе фильтр её не найдёт).</summary>
    [Fact]
    public async Task Blank_feature_is_stored_as_null()
    {
        var repo = await SyncAsync([], new PermissionDefinitionDto("A.View", "A", "Test", "   "));

        repo.Items.Single().Feature.ShouldBeNull();
    }
}

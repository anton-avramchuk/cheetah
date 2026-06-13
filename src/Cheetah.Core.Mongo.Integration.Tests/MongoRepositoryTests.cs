using Cheetah.Core.DataAccess.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Shouldly;

namespace Cheetah.Core.Mongo.Integration.Tests;

[Collection("Mongo")]
public class MongoRepositoryTests : IAsyncLifetime
{
    private readonly MongoFixture _fixture;

    public MongoRepositoryTests(MongoFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.DropAllAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Add_then_GetById_round_trips()
    {
        var product = Product.Create("Keyboard", 49.90m, 12);

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            repo.Add(product);
            (await repo.SaveChangesAsync()).ShouldBe(1);
        }

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            var loaded = await repo.GetByIdAsync(product.Id);

            loaded.ShouldNotBeNull();
            loaded!.Name.ShouldBe("Keyboard");
            loaded.Price.ShouldBe(49.90m);
            loaded.Stock.ShouldBe(12);
        }
    }

    [Fact]
    public async Task Update_persists_changes()
    {
        var product = Product.Create("Mouse", 19.90m, 5);
        await SaveAsync(product);

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            var loaded = await repo.GetByIdAsync(product.Id);
            loaded!.Restock(10);
            repo.Update(loaded);
            await repo.SaveChangesAsync();
        }

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            (await repo.GetByIdAsync(product.Id))!.Stock.ShouldBe(15);
        }
    }

    [Fact]
    public async Task Delete_removes_document()
    {
        var product = Product.Create("Cable", 4.50m, 100);
        await SaveAsync(product);

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            var loaded = await repo.GetByIdAsync(product.Id);
            repo.Delete(loaded!);
            await repo.SaveChangesAsync();
        }

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            (await repo.GetByIdAsync(product.Id)).ShouldBeNull();
        }
    }

    [Fact]
    public async Task GetBySpec_and_GetAll_filter_via_specifications()
    {
        await SaveAsync(Product.Create("Alpha", 1m, 1));
        await SaveAsync(Product.Create("Beta", 2m, 2, isActive: false));
        await SaveAsync(Product.Create("Gamma", 3m, 3));

        await using var scope = _fixture.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();

        var byName = await repo.GetBySpecAsync(new ProductByNameSpecification("Beta"));
        byName.ShouldNotBeNull();
        byName!.Name.ShouldBe("Beta");

        var active = await repo.GetAllAsync(new ActiveProductsSpecification());
        active.Count.ShouldBe(2);
        active.ShouldAllBe(p => p.IsActive);

        (await repo.ExistsAsync(new ProductByNameSpecification("Gamma"))).ShouldBeTrue();
        (await repo.ExistsAsync(new ProductByNameSpecification("Missing"))).ShouldBeFalse();
    }

    [Fact]
    public async Task SaveChanges_commits_multiple_writes_atomically()
    {
        var a = Product.Create("Batch-A", 1m, 1);
        var b = Product.Create("Batch-B", 2m, 2);

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            repo.Add(a);
            repo.Add(b);
            (await repo.SaveChangesAsync()).ShouldBe(2);
        }

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            (await repo.GetByIdAsync(a.Id)).ShouldNotBeNull();
            (await repo.GetByIdAsync(b.Id)).ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task SaveChanges_rolls_back_all_writes_on_failure()
    {
        var good = Product.Create("Good", 1m, 1);
        var dup = Product.Create("Dup", 2m, 2);

        // Pre-insert a document with the same _id so the second insert violates the unique _id index.
        await SaveAsync(dup);

        var collision = Product.Create("Collision", 3m, 3);
        ForceId(collision, dup.Id);

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            repo.Add(good);
            repo.Add(collision); // duplicate _id -> whole transaction aborts
            await Should.ThrowAsync<MongoException>(async () => await repo.SaveChangesAsync());
        }

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            // "good" must NOT be persisted because the transaction was rolled back.
            (await repo.GetByIdAsync(good.Id)).ShouldBeNull();
        }
    }

    [Fact]
    public async Task Empty_SaveChanges_returns_zero()
    {
        await using var scope = _fixture.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
        (await repo.SaveChangesAsync()).ShouldBe(0);
    }

    private async Task SaveAsync(Product product)
    {
        await using var scope = _fixture.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
        repo.Add(product);
        await repo.SaveChangesAsync();
    }

    private static void ForceId(Product product, Guid id)
    {
        var field = typeof(Cheetah.Core.Domain.Entity<Guid>)
            .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        field.SetValue(product, id);
    }
}

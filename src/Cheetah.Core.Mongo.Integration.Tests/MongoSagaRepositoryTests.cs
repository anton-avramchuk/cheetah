using Cheetah.Saga;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cheetah.Core.Mongo.Integration.Tests;

[Collection("Mongo")]
public class MongoSagaRepositoryTests : IAsyncLifetime
{
    private readonly MongoFixture _fixture;

    public MongoSagaRepositoryTests(MongoFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.DropAllAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static SagaInstance NewInstance(string correlation)
        => new()
        {
            SagaType = "OrderSaga",
            CorrelationKey = correlation,
            Status = SagaStatus.Running,
            DataType = "X",
            DataJson = "{}",
        };

    [Fact]
    public async Task Add_then_Find_round_trips()
    {
        var instance = NewInstance("order-1");

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ISagaRepository>();
            await repo.AddAsync(instance);
            await repo.SaveChangesAsync();
        }

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ISagaRepository>();
            var loaded = await repo.FindAsync("OrderSaga", "order-1");
            loaded.ShouldNotBeNull();
            loaded!.Status.ShouldBe(SagaStatus.Running);
        }
    }

    [Fact]
    public async Task Concurrent_update_raises_SagaConcurrencyException()
    {
        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ISagaRepository>();
            await repo.AddAsync(NewInstance("order-2"));
            await repo.SaveChangesAsync();
        }

        // Two independent scopes load the same instance at the same Version.
        var scopeA = _fixture.CreateScope();
        var scopeB = _fixture.CreateScope();
        var repoA = scopeA.ServiceProvider.GetRequiredService<ISagaRepository>();
        var repoB = scopeB.ServiceProvider.GetRequiredService<ISagaRepository>();

        var a = await repoA.FindAsync("OrderSaga", "order-2");
        var b = await repoB.FindAsync("OrderSaga", "order-2");

        a!.Status = SagaStatus.Completed;
        await repoA.SaveChangesAsync(); // wins, Version 0 -> 1

        b!.Status = SagaStatus.Failed;
        await Should.ThrowAsync<SagaConcurrencyException>(async () => await repoB.SaveChangesAsync());

        await scopeA.DisposeAsync();
        await scopeB.DisposeAsync();
    }

    [Fact]
    public async Task Version_increments_on_update()
    {
        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ISagaRepository>();
            await repo.AddAsync(NewInstance("order-3"));
            await repo.SaveChangesAsync();
        }

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ISagaRepository>();
            var loaded = await repo.FindAsync("OrderSaga", "order-3");
            loaded!.Status = SagaStatus.Completed;
            await repo.SaveChangesAsync();
        }

        await using (var scope = _fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ISagaRepository>();
            (await repo.FindAsync("OrderSaga", "order-3"))!.Version.ShouldBe(1);
        }
    }
}

using Cheetah.Core.Dapper.Querying;
using Cheetah.Core.DataAccess.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cheetah.Core.Dapper.Integration.Tests;

[Collection("Dapper.Postgres")]
public class DapperUnitOfWorkTests
{
    private readonly PostgresDapperFixture _fixture;

    public DapperUnitOfWorkTests(PostgresDapperFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SaveChanges_Flushes_Multiple_Writes_Atomically()
    {
        var a = Product.Create("Atomic-A", 1m, 1);
        var b = Product.Create("Atomic-B", 2m, 2);

        await using (var scope = _fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            repository.Add(a);
            repository.Add(b);
            var affected = await repository.SaveChangesAsync();
            affected.ShouldBe(2);
        }

        (await _fixture.ReadRawAsync(a.Id)).ShouldNotBeNull();
        (await _fixture.ReadRawAsync(b.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task SaveChanges_Rolls_Back_All_Writes_When_One_Fails()
    {
        var existing = Product.Create("RB-existing", 1m, 1);
        await using (var seedScope = _fixture.CreateScope())
        {
            var seedRepo = seedScope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            seedRepo.Add(existing);
            await seedRepo.SaveChangesAsync();
        }

        var fresh = Product.Create("RB-fresh", 1m, 1);
        var duplicate = Product.CreateWithId(existing.Id, "RB-duplicate", 1m, 1); // PK collision

        await using (var scope = _fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            repository.Add(fresh);
            repository.Add(duplicate);

            await Should.ThrowAsync<Exception>(async () => await repository.SaveChangesAsync());
        }

        // The fresh insert in the same transaction must have been rolled back.
        (await _fixture.ReadRawAsync(fresh.Id)).ShouldBeNull();
        (await _fixture.ReadRawAsync(existing.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task SaveChanges_With_No_Pending_Operations_Returns_Zero()
    {
        await using var scope = _fixture.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<Cheetah.Core.Dapper.UnitOfWork.IDapperUnitOfWork>();

        (await unitOfWork.SaveChangesAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task QueryExecutor_Returns_Projection()
    {
        await _fixture.TruncateAsync();
        await using (var scope = _fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            repository.Add(Product.Create("Proj-active", 12.50m, 3));
            var inactive = Product.Create("Proj-inactive", 99m, 0);
            inactive.Deactivate();
            repository.Add(inactive);
            await repository.SaveChangesAsync();
        }

        await using var queryScope = _fixture.CreateScope();
        var executor = queryScope.ServiceProvider.GetRequiredService<IDapperQueryExecutor>();

        var rows = await executor.QueryAsync<ProductRow>(
            "SELECT name, price FROM products WHERE is_active = @active ORDER BY name",
            new { active = true });

        rows.Count.ShouldBe(1);
        rows[0].Name.ShouldBe("Proj-active");
        rows[0].Price.ShouldBe(12.50m);
    }

    private sealed record ProductRow(string Name, decimal Price);
}

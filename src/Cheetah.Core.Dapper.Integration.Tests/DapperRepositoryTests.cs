using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cheetah.Core.Dapper.Integration.Tests;

[Collection("Dapper.Postgres")]
public class DapperRepositoryTests
{
    private readonly PostgresDapperFixture _fixture;

    public DapperRepositoryTests(PostgresDapperFixture fixture) => _fixture = fixture;

    private async Task<IReadOnlyList<Guid>> SeedAsync(params Product[] products)
    {
        await using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
        foreach (var product in products)
            repository.Add(product);
        await repository.SaveChangesAsync();
        return products.Select(p => p.Id).ToList();
    }

    [Fact]
    public async Task Add_Then_GetById_Round_Trips_All_Columns()
    {
        var product = Product.Create("Keyboard", 49.90m, 7);
        await SeedAsync(product);

        await using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();

        var loaded = await repository.GetByIdAsync(product.Id);

        loaded.ShouldNotBeNull();
        loaded.Id.ShouldBe(product.Id);
        loaded.Name.ShouldBe("Keyboard");
        loaded.Price.ShouldBe(49.90m);
        loaded.Stock.ShouldBe(7);
        loaded.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task GetById_Returns_Null_When_Missing()
    {
        await using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();

        var loaded = await repository.GetByIdAsync(Guid.NewGuid());

        loaded.ShouldBeNull();
    }

    [Fact]
    public async Task Update_Persists_Changed_Columns()
    {
        var product = Product.Create("Mouse", 19.00m, 10);
        await SeedAsync(product);

        await using (var scope = _fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            var loaded = await repository.GetByIdAsync(product.Id);
            loaded!.Deactivate();
            loaded.SetStock(0);
            repository.Update(loaded);
            await repository.SaveChangesAsync();
        }

        var raw = await _fixture.ReadRawAsync(product.Id);
        raw.ShouldNotBeNull();
        raw.Value.Stock.ShouldBe(0);
        raw.Value.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Delete_Removes_The_Row()
    {
        var product = Product.Create("Cable", 5.00m, 100);
        await SeedAsync(product);

        await using (var scope = _fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();
            var loaded = await repository.GetByIdAsync(product.Id);
            repository.Delete(loaded!);
            await repository.SaveChangesAsync();
        }

        (await _fixture.ReadRawAsync(product.Id)).ShouldBeNull();
    }

    [Fact]
    public async Task GetBySpec_Filters_Through_Specification()
    {
        await _fixture.TruncateAsync();
        var active = Product.Create("Active-spec", 1m, 1);
        var inactive = Product.Create("Inactive-spec", 1m, 1);
        inactive.Deactivate();
        await SeedAsync(active, inactive);

        await using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();

        var spec = new ExpressionSpecification<Product>(p => p.Name == "Active-spec" && p.IsActive);
        var found = await repository.GetBySpecAsync(spec);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(active.Id);
    }

    [Fact]
    public async Task GetAll_With_Spec_Returns_Matching_Set()
    {
        await _fixture.TruncateAsync();
        await SeedAsync(
            Product.Create("Cheap-1", 5m, 1),
            Product.Create("Cheap-2", 8m, 1),
            Product.Create("Expensive", 500m, 1));

        await using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();

        var spec = new ExpressionSpecification<Product>(p => p.Price < 10m);
        var result = await repository.GetAllAsync(spec);

        result.Count.ShouldBe(2);
        result.ShouldAllBe(p => p.Price < 10m);
    }

    [Fact]
    public async Task GetAll_Without_Spec_Returns_Everything()
    {
        await _fixture.TruncateAsync();
        await SeedAsync(
            Product.Create("All-1", 1m, 1),
            Product.Create("All-2", 1m, 1));

        await using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();

        var result = await repository.GetAllAsync();

        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Exists_Reflects_Presence()
    {
        await _fixture.TruncateAsync();
        var product = Product.Create("Existing", 1m, 1);
        await SeedAsync(product);

        await using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();

        (await repository.ExistsAsync(new ExpressionSpecification<Product>(p => p.Name == "Existing")))
            .ShouldBeTrue();
        (await repository.ExistsAsync(new ExpressionSpecification<Product>(p => p.Name == "Nope")))
            .ShouldBeFalse();
    }

    [Fact]
    public async Task AsQueryable_Is_Not_Supported()
    {
        await using var scope = _fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Product, Guid>>();

        Should.Throw<NotSupportedException>(() => repository.AsQueryable());
        Should.Throw<NotSupportedException>(() => repository.AsNoTrackingQueryable());
    }
}

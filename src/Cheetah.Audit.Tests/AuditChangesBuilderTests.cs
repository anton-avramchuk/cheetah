using System.Text.Json;
using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Cheetah.Audit.Tests;

public class AuditChangesBuilderTests
{
    private static TestAuditDbContext CreateContext()
    {
        var opts = new DbContextOptionsBuilder<TestAuditDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestAuditDbContext(opts);
    }

    [Fact]
    public void Created_содержит_все_не_null_свойства_как_new()
    {
        using var ctx = CreateContext();
        var customer = new TestCustomer { Name = "Ann", Email = "a@x", PasswordHash = "secret" };
        ctx.Customers.Add(customer);

        var entry = ctx.Entry(customer);
        var json = AuditChangesBuilder.Build(entry, AuditAction.Created);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("Name").GetProperty("new").GetString().ShouldBe("Ann");
        doc.RootElement.GetProperty("Email").GetProperty("new").GetString().ShouldBe("a@x");
        doc.RootElement.GetProperty("PasswordHash").GetProperty("new").GetString().ShouldBe("***");
    }

    [Fact]
    public void Created_исключает_NotAudited_свойства()
    {
        using var ctx = CreateContext();
        var customer = new TestCustomer { Name = "Ann", LoginCount = 42 };
        ctx.Customers.Add(customer);

        var json = AuditChangesBuilder.Build(ctx.Entry(customer), AuditAction.Created);
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("LoginCount", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task Updated_содержит_только_изменённые_свойства_с_old_и_new()
    {
        using var ctx = CreateContext();
        var customer = new TestCustomer { Name = "Ann", Email = "a@x" };
        ctx.Customers.Add(customer);
        await ctx.SaveChangesAsync();

        customer.Name = "Anna";
        // Email не трогаем
        var json = AuditChangesBuilder.Build(ctx.Entry(customer), AuditAction.Updated);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("Name").GetProperty("old").GetString().ShouldBe("Ann");
        doc.RootElement.GetProperty("Name").GetProperty("new").GetString().ShouldBe("Anna");
        doc.RootElement.TryGetProperty("Email", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task Updated_маскирует_Sensitive_значения_в_diff()
    {
        using var ctx = CreateContext();
        var customer = new TestCustomer { Name = "Ann", PasswordHash = "old-secret" };
        ctx.Customers.Add(customer);
        await ctx.SaveChangesAsync();

        customer.PasswordHash = "new-secret";
        var json = AuditChangesBuilder.Build(ctx.Entry(customer), AuditAction.Updated);
        using var doc = JsonDocument.Parse(json);

        var pwd = doc.RootElement.GetProperty("PasswordHash");
        pwd.GetProperty("old").GetString().ShouldBe("***");
        pwd.GetProperty("new").GetString().ShouldBe("***");
        json.ShouldNotContain("old-secret");
        json.ShouldNotContain("new-secret");
    }

    [Fact]
    public async Task Deleted_содержит_old_значения()
    {
        using var ctx = CreateContext();
        var customer = new TestCustomer { Name = "Ann", Email = "a@x" };
        ctx.Customers.Add(customer);
        await ctx.SaveChangesAsync();

        ctx.Customers.Remove(customer);
        var json = AuditChangesBuilder.Build(ctx.Entry(customer), AuditAction.Deleted);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("Name").GetProperty("old").GetString().ShouldBe("Ann");
        doc.RootElement.GetProperty("Name").TryGetProperty("new", out var n).ShouldBeTrue();
        n.ValueKind.ShouldBe(JsonValueKind.Null);
    }
}

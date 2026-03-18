namespace Cheetah.Core.Identity.Domain.Tests;

public class IdentityRoleTests
{
    // --- Create ---

    [Fact]
    public void Create_WithValidName_SetsNameAndNormalizedName()
    {
        var role = TestRole.Create("admin");

        role.Name.ShouldBe("admin");
        role.NormalizedName.ShouldBe("ADMIN");
    }

    [Fact]
    public void Create_GeneratesNewId()
    {
        var a = TestRole.Create("admin");
        var b = TestRole.Create("admin");

        a.Id.ShouldNotBe(b.Id);
    }

    [Fact]
    public void Create_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();
        var role = TestRole.Create(id, "admin");

        role.Id.ShouldBe(id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_Throws(string? name)
    {
        Should.Throw<ArgumentException>(() => TestRole.Create(name!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithExplicitId_WithInvalidName_Throws(string? name)
    {
        Should.Throw<ArgumentException>(() => TestRole.Create(Guid.NewGuid(), name!));
    }

    // --- ChangeName ---

    [Fact]
    public void ChangeName_UpdatesNameAndNormalizedName()
    {
        var role = TestRole.Create("admin");

        role.ChangeName("moderator");

        role.Name.ShouldBe("moderator");
        role.NormalizedName.ShouldBe("MODERATOR");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeName_WithInvalidName_Throws(string? name)
    {
        var role = TestRole.Create("admin");

        Should.Throw<ArgumentException>(() => role.ChangeName(name!));
    }

    // --- Claims ---

    [Fact]
    public void AddClaim_AppendsClaim()
    {
        var role = TestRole.Create("admin");
        var claim = new Claim("permission", "read");

        role.AddClaim(claim);

        role.Claims.Count.ShouldBe(1);
        role.Claims.First().ClaimType.ShouldBe("permission");
        role.Claims.First().ClaimValue.ShouldBe("read");
    }

    [Fact]
    public void AddClaim_WithNullClaim_Throws()
    {
        var role = TestRole.Create("admin");

        Should.Throw<ArgumentNullException>(() => role.AddClaim(null!));
    }

    [Fact]
    public void RemoveClaim_RemovesMatchingClaim()
    {
        var role = TestRole.Create("admin");
        var claim = new Claim("permission", "read");
        role.AddClaim(claim);

        role.RemoveClaim(claim);

        role.Claims.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveClaim_DoesNotRemoveNonMatchingClaims()
    {
        var role = TestRole.Create("admin");
        role.AddClaim(new Claim("permission", "read"));
        role.AddClaim(new Claim("permission", "write"));

        role.RemoveClaim(new Claim("permission", "read"));

        role.Claims.Count.ShouldBe(1);
        role.Claims.First().ClaimValue.ShouldBe("write");
    }

    [Fact]
    public void RemoveClaim_WithNullClaim_Throws()
    {
        var role = TestRole.Create("admin");

        Should.Throw<ArgumentNullException>(() => role.RemoveClaim(null!));
    }

    // --- Audit ---

    [Fact]
    public void ImplementsICreateAtEntity()
    {
        TestRole.Create("admin").ShouldBeAssignableTo<Cheetah.Core.Domain.ICreateAtEntity>();
    }

    [Fact]
    public void ImplementsIUpdatedAtEntity()
    {
        TestRole.Create("admin").ShouldBeAssignableTo<Cheetah.Core.Domain.IUpdatedAtEntity>();
    }
}

namespace Cheetah.Core.Identity.Domain.Tests;

public class IdentityUserTests
{
    private static TestUser CreateUser(
        string userName = "john",
        string email = "john@example.com")
        => TestUser.Create(userName, email);

    private static TestRole CreateRole(string name = "admin")
        => TestRole.Create(name);

    // --- Create ---

    [Fact]
    public void Create_SetsUserNameAndEmail()
    {
        var user = CreateUser("alice", "alice@example.com");

        user.UserName.ShouldBe("alice");
        user.Email.ShouldBe("alice@example.com");
    }

    [Fact]
    public void Create_NormalizesUserNameAndEmail()
    {
        var user = CreateUser("Alice", "Alice@Example.com");

        user.NormalizedUserName.ShouldBe("ALICE");
        user.NormalizedEmail.ShouldBe("ALICE@EXAMPLE.COM");
    }

    [Fact]
    public void Create_GeneratesNonEmptySecurityStamp()
    {
        var user = CreateUser();

        user.SecurityStamp.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_GeneratesNewId()
    {
        var a = CreateUser();
        var b = CreateUser();

        a.Id.ShouldNotBe(b.Id);
    }

    [Fact]
    public void Create_EmailConfirmedIsFalseByDefault()
    {
        CreateUser().EmailConfirmed.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidUserName_Throws(string? userName)
    {
        Should.Throw<ArgumentException>(() => CreateUser(userName!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_Throws(string? email)
    {
        Should.Throw<ArgumentException>(() => CreateUser(email: email!));
    }

    // --- UserName ---

    [Fact]
    public void ChangeUserName_UpdatesUserNameAndNormalized()
    {
        var user = CreateUser("john");

        user.ChangeUserName("jane");

        user.UserName.ShouldBe("jane");
        user.NormalizedUserName.ShouldBe("JANE");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeUserName_WithInvalidValue_Throws(string? name)
    {
        var user = CreateUser();

        Should.Throw<ArgumentException>(() => user.ChangeUserName(name!));
    }

    // --- Email ---

    [Fact]
    public void ChangeEmail_UpdatesEmailAndNormalized()
    {
        var user = CreateUser();

        user.ChangeEmail("new@example.com");

        user.Email.ShouldBe("new@example.com");
        user.NormalizedEmail.ShouldBe("NEW@EXAMPLE.COM");
    }

    [Fact]
    public void ChangeEmail_ResetsEmailConfirmed()
    {
        var user = CreateUser();
        user.ConfirmEmail();
        user.EmailConfirmed.ShouldBeTrue();

        user.ChangeEmail("new@example.com");

        user.EmailConfirmed.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeEmail_WithInvalidValue_Throws(string? email)
    {
        var user = CreateUser();

        Should.Throw<ArgumentException>(() => user.ChangeEmail(email!));
    }

    [Fact]
    public void ConfirmEmail_SetsEmailConfirmedTrue()
    {
        var user = CreateUser();

        user.ConfirmEmail();

        user.EmailConfirmed.ShouldBeTrue();
    }

    // --- Password & Security ---

    [Fact]
    public void SetPasswordHash_SetsValue()
    {
        var user = CreateUser();

        user.SetPasswordHash("hashed");

        user.PasswordHash.ShouldBe("hashed");
    }

    [Fact]
    public void SetPasswordHash_AllowsNull()
    {
        var user = CreateUser();
        user.SetPasswordHash("hashed");

        user.SetPasswordHash(null);

        user.PasswordHash.ShouldBeNull();
    }

    [Fact]
    public void RefreshSecurityStamp_ChangesStamp()
    {
        var user = CreateUser();
        var original = user.SecurityStamp;

        user.RefreshSecurityStamp();

        user.SecurityStamp.ShouldNotBe(original);
        user.SecurityStamp.ShouldNotBeNullOrWhiteSpace();
    }

    // --- Lockout ---

    [Fact]
    public void SetLockoutEnd_SetsValue()
    {
        var user = CreateUser();
        var until = DateTimeOffset.UtcNow.AddHours(1);

        user.SetLockoutEnd(until);

        user.LockoutEnd.ShouldBe(until);
    }

    [Fact]
    public void SetLockoutEnabled_SetsValue()
    {
        var user = CreateUser();

        user.SetLockoutEnabled(true);

        user.LockoutEnabled.ShouldBeTrue();
    }

    [Fact]
    public void IncrementAccessFailedCount_Increments()
    {
        var user = CreateUser();

        user.IncrementAccessFailedCount();
        user.IncrementAccessFailedCount();

        user.AccessFailedCount.ShouldBe(2);
    }

    [Fact]
    public void ResetAccessFailedCount_SetsZero()
    {
        var user = CreateUser();
        user.IncrementAccessFailedCount();
        user.IncrementAccessFailedCount();

        user.ResetAccessFailedCount();

        user.AccessFailedCount.ShouldBe(0);
    }

    // --- Roles ---

    [Fact]
    public void AddRole_AddsRole()
    {
        var user = CreateUser();
        var role = CreateRole();

        user.AddRole(role);

        user.Roles.Count.ShouldBe(1);
        user.Roles.First().RoleId.ShouldBe(role.Id);
    }

    [Fact]
    public void AddRole_Duplicate_IsIgnored()
    {
        var user = CreateUser();
        var role = CreateRole();

        user.AddRole(role);
        user.AddRole(role);

        user.Roles.Count.ShouldBe(1);
    }

    [Fact]
    public void AddRole_WithNull_Throws()
    {
        var user = CreateUser();

        Should.Throw<ArgumentNullException>(() => user.AddRole(null!));
    }

    [Fact]
    public void RemoveRole_RemovesRole()
    {
        var user = CreateUser();
        var role = CreateRole();
        user.AddRole(role);

        user.RemoveRole(role);

        user.Roles.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveRole_WithNull_Throws()
    {
        var user = CreateUser();

        Should.Throw<ArgumentNullException>(() => user.RemoveRole(null!));
    }

    [Fact]
    public void IsInRole_ById_ReturnsTrueWhenPresent()
    {
        var user = CreateUser();
        var role = CreateRole();
        user.AddRole(role);

        user.IsInRole(role.Id).ShouldBeTrue();
    }

    [Fact]
    public void IsInRole_ById_ReturnsFalseWhenAbsent()
    {
        var user = CreateUser();

        user.IsInRole(Guid.NewGuid()).ShouldBeFalse();
    }

    [Fact]
    public void IsInRole_ByRole_ReturnsTrueWhenPresent()
    {
        var user = CreateUser();
        var role = CreateRole();
        user.AddRole(role);

        user.IsInRole(role).ShouldBeTrue();
    }

    [Fact]
    public void IsInRole_ByRole_WithNull_Throws()
    {
        var user = CreateUser();

        Should.Throw<ArgumentNullException>(() => user.IsInRole(null!));
    }

    // --- Claims ---

    [Fact]
    public void AddClaim_AppendsClaim()
    {
        var user = CreateUser();
        var claim = new Claim("role", "admin");

        user.AddClaim(claim);

        user.Claims.Count.ShouldBe(1);
        user.Claims.First().ClaimType.ShouldBe("role");
        user.Claims.First().ClaimValue.ShouldBe("admin");
    }

    [Fact]
    public void AddClaim_AllowsDuplicateClaims()
    {
        // Intentional: claims with the same type but different values are valid
        // (standard ASP.NET Identity / multi-value claim behaviour).
        var user = CreateUser();

        user.AddClaim(new Claim("permission", "read"));
        user.AddClaim(new Claim("permission", "write"));

        user.Claims.Count.ShouldBe(2);
        user.Claims.Select(c => c.ClaimValue).ShouldBe(["read", "write"], ignoreOrder: true);
    }

    [Fact]
    public void AddClaim_WithNull_Throws()
    {
        var user = CreateUser();

        Should.Throw<ArgumentNullException>(() => user.AddClaim(null!));
    }

    [Fact]
    public void AddClaims_AddsAll()
    {
        var user = CreateUser();
        var claims = new[] { new Claim("a", "1"), new Claim("b", "2") };

        user.AddClaims(claims);

        user.Claims.Count.ShouldBe(2);
    }

    [Fact]
    public void AddClaims_WithNull_Throws()
    {
        var user = CreateUser();

        Should.Throw<ArgumentNullException>(() => user.AddClaims(null!));
    }

    [Fact]
    public void FindClaim_ReturnsMatchingClaim()
    {
        var user = CreateUser();
        var claim = new Claim("role", "admin");
        user.AddClaim(claim);

        var found = user.FindClaim(claim);

        found.ShouldNotBeNull();
        found.ClaimType.ShouldBe("role");
        found.ClaimValue.ShouldBe("admin");
    }

    [Fact]
    public void FindClaim_WhenNotFound_ReturnsNull()
    {
        var user = CreateUser();

        var found = user.FindClaim(new Claim("role", "admin"));

        found.ShouldBeNull();
    }

    [Fact]
    public void FindClaim_WithNull_Throws()
    {
        var user = CreateUser();

        Should.Throw<ArgumentNullException>(() => user.FindClaim(null!));
    }

    [Fact]
    public void ReplaceClaim_UpdatesClaimValue()
    {
        var user = CreateUser();
        var original = new Claim("role", "user");
        user.AddClaim(original);

        user.ReplaceClaim(original, new Claim("role", "admin"));

        user.Claims.First().ClaimValue.ShouldBe("admin");
    }

    [Fact]
    public void ReplaceClaim_WithNullClaim_Throws()
    {
        var user = CreateUser();

        Should.Throw<ArgumentNullException>(() => user.ReplaceClaim(null!, new Claim("a", "b")));
    }

    [Fact]
    public void ReplaceClaim_WithNullNewClaim_Throws()
    {
        var user = CreateUser();

        Should.Throw<ArgumentNullException>(() => user.ReplaceClaim(new Claim("a", "b"), null!));
    }

    [Fact]
    public void RemoveClaim_RemovesMatchingClaim()
    {
        var user = CreateUser();
        var claim = new Claim("role", "admin");
        user.AddClaim(claim);

        user.RemoveClaim(claim);

        user.Claims.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveClaim_WithNull_Throws()
    {
        var user = CreateUser();

        Should.Throw<ArgumentNullException>(() => user.RemoveClaim(null!));
    }

    [Fact]
    public void RemoveClaims_RemovesAll()
    {
        var user = CreateUser();
        var claims = new[] { new Claim("a", "1"), new Claim("b", "2") };
        user.AddClaims(claims);

        user.RemoveClaims(claims);

        user.Claims.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveClaims_WithNull_Throws()
    {
        var user = CreateUser();

        Should.Throw<ArgumentNullException>(() => user.RemoveClaims(null!));
    }

    // --- Audit ---

    [Fact]
    public void ImplementsICreateAtEntity()
    {
        CreateUser().ShouldBeAssignableTo<Cheetah.Core.Domain.ICreateAtEntity>();
    }

    [Fact]
    public void ImplementsIUpdatedAtEntity()
    {
        CreateUser().ShouldBeAssignableTo<Cheetah.Core.Domain.IUpdatedAtEntity>();
    }
}

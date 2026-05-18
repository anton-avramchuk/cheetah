using Cheetah.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Cheetah.Permissions.Tests;

public class PermissionPolicyProviderTests
{
    private readonly PermissionPolicyProvider _sut = new(Options.Create(new AuthorizationOptions()));

    [Fact]
    public async Task GetPolicy_Returns_Policy_With_PermissionRequirement_For_Prefixed_Name()
    {
        var policy = await _sut.GetPolicyAsync("permission:Documents.Sign");

        policy.ShouldNotBeNull();
        var req = policy.Requirements.OfType<PermissionRequirement>().SingleOrDefault();
        req.ShouldNotBeNull();
        req!.Permission.ShouldBe("Documents.Sign");
    }

    [Fact]
    public async Task GetPolicy_NonPrefixed_Falls_Back_To_Default_Provider()
    {
        var policy = await _sut.GetPolicyAsync("SomeRegularPolicy");
        policy.ShouldBeNull(); // в default provider такой не зарегистрирован
    }
}

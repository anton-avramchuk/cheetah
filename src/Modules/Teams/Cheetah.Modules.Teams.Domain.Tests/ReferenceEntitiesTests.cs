using Cheetah.Modules.Teams.Domain.Entities;
using Shouldly;

namespace Cheetah.Modules.Teams.Domain.Tests;

public class ReferenceEntitiesTests
{
    [Fact]
    public void TeamRole_create_trims_name()
    {
        var role = TeamRole.Create("  Lead  ");

        role.Id.ShouldNotBe(Guid.Empty);
        role.Name.ShouldBe("Lead");
    }

    [Fact]
    public void TeamRole_rename_changes_name()
    {
        var role = TeamRole.Create("Lead");
        role.Rename("Manager");
        role.Name.ShouldBe("Manager");
    }

    [Fact]
    public void TeamMember_create_sets_name()
    {
        var member = TeamMember.Create("John");

        member.Id.ShouldNotBe(Guid.Empty);
        member.Name.ShouldBe("John");
    }

    [Fact]
    public void TeamMember_rename_changes_name()
    {
        var member = TeamMember.Create("John");
        member.Rename("Jane");
        member.Name.ShouldBe("Jane");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void TeamMember_blank_name_throws(string name)
        => Should.Throw<ArgumentException>(() => TeamMember.Create(name));
}

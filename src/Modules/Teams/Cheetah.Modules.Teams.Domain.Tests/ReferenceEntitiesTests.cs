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
    public void TeamMember_create_with_user_link()
    {
        var userId = Guid.NewGuid();
        var member = TeamMember.Create("John", userId);

        member.Name.ShouldBe("John");
        member.UserId.ShouldBe(userId);
    }

    [Fact]
    public void TeamMember_link_user_updates_reference()
    {
        var member = TeamMember.Create("John");
        member.UserId.ShouldBeNull();

        var userId = Guid.NewGuid();
        member.LinkUser(userId);
        member.UserId.ShouldBe(userId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void TeamMember_blank_name_throws(string name)
        => Should.Throw<ArgumentException>(() => TeamMember.Create(name));
}

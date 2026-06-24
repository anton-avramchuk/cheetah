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

    // Участники (TeamMember) заводятся только из Identity — см. UserDirectorySyncTests.
}

using Cheetah.Modules.Teams.DomainEvents;
using Shouldly;

namespace Cheetah.Modules.Teams.Domain.Tests;

public class TeamBaseTests
{
    [Fact]
    public void Create_sets_invariants_and_raises_event()
    {
        var team = TestTeam.Create(" Alpha ", "R&D");

        team.Id.ShouldNotBe(Guid.Empty);
        team.Name.ShouldBe("Alpha");
        team.IsActive.ShouldBeTrue();
        team.Department.ShouldBe("R&D");
        team.DomainEvents.ShouldContain(e => e is TeamCreatedIntegrationEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
        => Should.Throw<ArgumentException>(() => TestTeam.Create(name));

    [Fact]
    public void Rename_trims_and_raises_event()
    {
        var team = TestTeam.Create("Alpha");
        team.ClearDomainEvents();

        team.Rename(" Beta ");

        team.Name.ShouldBe("Beta");
        team.DomainEvents.ShouldContain(e => e is TeamUpdatedIntegrationEvent);
    }

    [Fact]
    public void Deactivate_then_activate_toggles_and_is_idempotent()
    {
        var team = TestTeam.Create("Alpha");
        team.ClearDomainEvents();

        team.Deactivate();
        team.IsActive.ShouldBeFalse();
        team.DomainEvents.ShouldContain(e => e is TeamDeactivatedIntegrationEvent);

        team.ClearDomainEvents();
        team.Deactivate(); // no-op
        team.DomainEvents.ShouldBeEmpty();

        team.Activate();
        team.IsActive.ShouldBeTrue();
        team.DomainEvents.ShouldContain(e => e is TeamActivatedIntegrationEvent);
    }

    [Fact]
    public void AddMember_adds_membership_and_raises_event()
    {
        var team = TestTeam.Create("Alpha");
        var memberId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        team.ClearDomainEvents();

        team.AddMember(memberId, roleId);

        var membership = team.Members.ShouldHaveSingleItem();
        membership.MemberId.ShouldBe(memberId);
        membership.RoleId.ShouldBe(roleId);
        team.DomainEvents.ShouldContain(e => e is TeamMemberAddedIntegrationEvent);
    }

    [Fact]
    public void AddMember_twice_changes_role_without_duplicate()
    {
        var team = TestTeam.Create("Alpha");
        var memberId = Guid.NewGuid();
        var role1 = Guid.NewGuid();
        var role2 = Guid.NewGuid();

        team.AddMember(memberId, role1);
        team.ClearDomainEvents();
        team.AddMember(memberId, role2);

        team.Members.ShouldHaveSingleItem().RoleId.ShouldBe(role2);
        team.DomainEvents.ShouldContain(e => e is TeamMemberRoleChangedIntegrationEvent);
    }

    [Fact]
    public void ChangeMemberRole_for_missing_member_throws()
    {
        var team = TestTeam.Create("Alpha");

        Should.Throw<InvalidOperationException>(() => team.ChangeMemberRole(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void RemoveMember_removes_and_raises_event()
    {
        var team = TestTeam.Create("Alpha");
        var memberId = Guid.NewGuid();
        team.AddMember(memberId, Guid.NewGuid());
        team.ClearDomainEvents();

        team.RemoveMember(memberId);

        team.Members.ShouldBeEmpty();
        team.DomainEvents.ShouldContain(e => e is TeamMemberRemovedIntegrationEvent);
    }

    [Fact]
    public void RemoveMember_missing_is_noop()
    {
        var team = TestTeam.Create("Alpha");
        team.ClearDomainEvents();

        team.RemoveMember(Guid.NewGuid());

        team.DomainEvents.ShouldBeEmpty();
    }
}

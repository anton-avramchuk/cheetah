using Cheetah.Modules.Activities.DomainEvents;
using Cheetah.Modules.Activities.Shared;
using Shouldly;

namespace Cheetah.Modules.Activities.Domain.Tests;

public class ActivityBaseTests
{
    private static TestActivity NewTask(DateTimeOffset? dueAt = null)
        => TestActivity.Create(ActivityType.Task, "Call client", Guid.NewGuid(), Guid.NewGuid(),
            EntityRefKeys.Deal, Guid.NewGuid(), dueAt);

    [Fact]
    public void Create_SetsOpen_AndRaisesCreatedEvent()
    {
        var assignee = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var activity = TestActivity.Create(ActivityType.Call, "Intro call", assignee, Guid.NewGuid(),
            EntityRefKeys.Lead, entityId);

        activity.Id.ShouldNotBe(Guid.Empty);
        activity.Status.ShouldBe(ActivityStatus.Open);
        activity.Title.ShouldBe("Intro call");
        activity.EntityType.ShouldBe(EntityRefKeys.Lead);

        var created = activity.DomainEvents.OfType<ActivityCreatedIntegrationEvent>().ShouldHaveSingleItem();
        created.ActivityId.ShouldBe(activity.Id);
        created.AssigneeId.ShouldBe(assignee);
        created.EntityId.ShouldBe(entityId);
    }

    [Fact]
    public void Create_TrimsTitle()
        => TestActivity.Create(ActivityType.Task, "  Do it  ", Guid.NewGuid(), Guid.NewGuid(),
            EntityRefKeys.Deal, Guid.NewGuid()).Title.ShouldBe("Do it");

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankTitle_Throws(string title)
        => Should.Throw<ArgumentException>(() => TestActivity.Create(ActivityType.Task, title,
            Guid.NewGuid(), Guid.NewGuid(), EntityRefKeys.Deal, Guid.NewGuid()));

    [Fact]
    public void Create_BlankEntityType_Throws()
        => Should.Throw<ArgumentException>(() => TestActivity.Create(ActivityType.Task, "x",
            Guid.NewGuid(), Guid.NewGuid(), "  ", Guid.NewGuid()));

    [Fact]
    public void AddReminder_WithoutDueAt_Throws()
        => Should.Throw<InvalidOperationException>(() => NewTask().AddReminder(TimeSpan.FromMinutes(30), "email"));

    [Fact]
    public void AddReminder_WithDueAt_Adds()
    {
        var activity = NewTask(DateTimeOffset.UtcNow.AddDays(1));
        activity.AddReminder(TimeSpan.FromMinutes(15), "email");
        activity.Reminders.ShouldHaveSingleItem().Channel.ShouldBe("email");
    }

    [Fact]
    public void Complete_SetsDone_AndRaisesEvent()
    {
        var activity = NewTask();
        var by = Guid.NewGuid();

        activity.Complete(by, "done");

        activity.Status.ShouldBe(ActivityStatus.Done);
        activity.CompletedAt.ShouldNotBeNull();
        activity.Result.ShouldBe("done");
        activity.DomainEvents.OfType<ActivityCompletedIntegrationEvent>().ShouldHaveSingleItem()
            .CompletedBy.ShouldBe(by);
    }

    [Fact]
    public void Complete_Twice_IsIdempotent()
    {
        var activity = NewTask();
        activity.Complete(Guid.NewGuid());
        activity.Complete(Guid.NewGuid());
        activity.DomainEvents.OfType<ActivityCompletedIntegrationEvent>().Count().ShouldBe(1);
    }

    [Fact]
    public void Cancel_SetsCanceled_AndRaisesEvent()
    {
        var activity = NewTask();
        activity.Cancel();
        activity.Status.ShouldBe(ActivityStatus.Canceled);
        activity.DomainEvents.OfType<ActivityCanceledIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Reassign_ChangesAssignee_AndRaisesEvent()
    {
        var activity = NewTask();
        var newAssignee = Guid.NewGuid();

        activity.Reassign(newAssignee);

        activity.AssigneeId.ShouldBe(newAssignee);
        activity.DomainEvents.OfType<ActivityReassignedIntegrationEvent>().ShouldHaveSingleItem()
            .NewAssigneeId.ShouldBe(newAssignee);
    }

    [Fact]
    public void Reassign_SameValue_IsIdempotent()
    {
        var activity = NewTask();
        activity.Reassign(activity.AssigneeId);
        activity.DomainEvents.OfType<ActivityReassignedIntegrationEvent>().ShouldBeEmpty();
    }

    [Fact]
    public void Start_OpenToInProgress()
    {
        var activity = NewTask();
        activity.Start();
        activity.Status.ShouldBe(ActivityStatus.InProgress);
    }

    [Fact]
    public void TryMarkOverdue_OverdueOpen_RaisesEvent()
    {
        var activity = NewTask(DateTimeOffset.UtcNow.AddHours(-1));
        var marked = activity.TryMarkOverdue(DateTimeOffset.UtcNow);
        marked.ShouldBeTrue();
        activity.DomainEvents.OfType<ActivityOverdueIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void TryMarkOverdue_NotDue_NoEvent()
    {
        var activity = NewTask(DateTimeOffset.UtcNow.AddHours(1));
        activity.TryMarkOverdue(DateTimeOffset.UtcNow).ShouldBeFalse();
        activity.DomainEvents.OfType<ActivityOverdueIntegrationEvent>().ShouldBeEmpty();
    }

    [Fact]
    public void TryMarkOverdue_Completed_NoEvent()
    {
        var activity = NewTask(DateTimeOffset.UtcNow.AddHours(-1));
        activity.Complete(Guid.NewGuid());
        activity.TryMarkOverdue(DateTimeOffset.UtcNow).ShouldBeFalse();
    }

    [Fact]
    public void Update_ChangesBaseFields()
    {
        var activity = NewTask();
        var due = DateTimeOffset.UtcNow.AddDays(3);

        activity.Update("Renamed", "desc", ActivityPriority.High, due);

        activity.Title.ShouldBe("Renamed");
        activity.Description.ShouldBe("desc");
        activity.Priority.ShouldBe(ActivityPriority.High);
        activity.DueAt.ShouldBe(due);
    }

    [Fact]
    public void ExtensionField_IsIndependentOfBase()
    {
        var activity = NewTask();
        activity.SetCallOutcome("reached");
        activity.CallOutcome.ShouldBe("reached");
    }
}

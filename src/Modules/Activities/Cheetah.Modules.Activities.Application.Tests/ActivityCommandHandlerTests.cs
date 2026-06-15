using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Activities.Application.Activities;
using Cheetah.Modules.Activities.Application.Exceptions;
using Cheetah.Modules.Activities.DomainEvents;
using Cheetah.Modules.Activities.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Activities.Application.Tests;

public class ActivityCommandHandlerTests
{
    private readonly Mock<IRepository<TestActivity, Guid>> _repo = new();
    private readonly Mock<IEventBus> _eventBus = new();
    private readonly Mock<IStateMachineValidator<ActivityStatus>> _sm = new();

    [Fact]
    public async Task Create_AddsSavesAndPublishesCreatedEvent()
    {
        var handler = new CreateActivityCommandHandler<TestActivity, TestCreateRequest>(
            new TestActivityFactory(), _repo.Object, _eventBus.Object);

        var id = await handler.HandleAsync(
            new CreateActivityCommand<TestCreateRequest>(TestData.CreateRequest()));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.Is<TestActivity>(a => a.Title == "Call client")), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is ActivityCreatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_PassesExtensionField()
    {
        var handler = new CreateActivityCommandHandler<TestActivity, TestCreateRequest>(
            new TestActivityFactory(), _repo.Object, _eventBus.Object);

        var req = TestData.CreateRequest() with { CallOutcome = "reached" };
        await handler.HandleAsync(new CreateActivityCommand<TestCreateRequest>(req));

        _repo.Verify(r => r.Add(It.Is<TestActivity>(a => a.CallOutcome == "reached")), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestActivity?)null);
        var handler = new UpdateActivityCommandHandler<TestActivity, TestUpdateRequest>(_repo.Object);

        await Should.ThrowAsync<ActivityValidationException>(() =>
            handler.HandleAsync(new UpdateActivityCommand<TestUpdateRequest>(
                Guid.NewGuid(), new TestUpdateRequest { Title = "x" })).AsTask());
    }

    [Fact]
    public async Task Update_ChangesFields_AndSaves()
    {
        var activity = TestData.NewActivity();
        _repo.Setup(r => r.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        var handler = new UpdateActivityCommandHandler<TestActivity, TestUpdateRequest>(_repo.Object);

        await handler.HandleAsync(new UpdateActivityCommand<TestUpdateRequest>(
            activity.Id, new TestUpdateRequest { Title = "Renamed", Priority = ActivityPriority.High }));

        activity.Title.ShouldBe("Renamed");
        activity.Priority.ShouldBe(ActivityPriority.High);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Complete_ValidatesTransition_SetsDone_AndPublishes()
    {
        var activity = TestData.NewActivity();
        _repo.Setup(r => r.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        var handler = new CompleteActivityCommandHandler<TestActivity>(_repo.Object, _eventBus.Object, _sm.Object);
        var by = Guid.NewGuid();

        await handler.HandleAsync(new CompleteActivityCommand(activity.Id, by, "ok"));

        _sm.Verify(s => s.ValidateTransition(ActivityStatus.Open, ActivityStatus.Done), Times.Once);
        activity.Status.ShouldBe(ActivityStatus.Done);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is ActivityCompletedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_SetsCanceled_AndPublishes()
    {
        var activity = TestData.NewActivity();
        _repo.Setup(r => r.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        var handler = new CancelActivityCommandHandler<TestActivity>(_repo.Object, _eventBus.Object, _sm.Object);

        await handler.HandleAsync(new CancelActivityCommand(activity.Id));

        _sm.Verify(s => s.ValidateTransition(ActivityStatus.Open, ActivityStatus.Canceled), Times.Once);
        activity.Status.ShouldBe(ActivityStatus.Canceled);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is ActivityCanceledIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reassign_ChangesAssignee_AndPublishes()
    {
        var activity = TestData.NewActivity();
        _repo.Setup(r => r.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        var handler = new ReassignActivityCommandHandler<TestActivity>(_repo.Object, _eventBus.Object);
        var newAssignee = Guid.NewGuid();

        await handler.HandleAsync(new ReassignActivityCommand(activity.Id, newAssignee));

        activity.AssigneeId.ShouldBe(newAssignee);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is ActivityReassignedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Start_ValidatesTransition_AndSaves()
    {
        var activity = TestData.NewActivity();
        _repo.Setup(r => r.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        var handler = new StartActivityCommandHandler<TestActivity>(_repo.Object, _sm.Object);

        await handler.HandleAsync(new StartActivityCommand(activity.Id));

        _sm.Verify(s => s.ValidateTransition(ActivityStatus.Open, ActivityStatus.InProgress), Times.Once);
        activity.Status.ShouldBe(ActivityStatus.InProgress);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

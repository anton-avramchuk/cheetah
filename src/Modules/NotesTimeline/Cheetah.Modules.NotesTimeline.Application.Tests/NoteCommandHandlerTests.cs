using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.NotesTimeline.Application.Notes;
using Cheetah.Modules.NotesTimeline.Application.Exceptions;
using Cheetah.Modules.NotesTimeline.DomainEvents;
using Moq;
using Shouldly;

namespace Cheetah.Modules.NotesTimeline.Application.Tests;

public class NoteCommandHandlerTests
{
    private readonly Mock<IRepository<TestNote, Guid>> _repo = new();
    private readonly Mock<IEventBus> _eventBus = new();

    [Fact]
    public async Task Create_AddsSavesAndPublishesCreatedEvent()
    {
        var handler = new CreateNoteCommandHandler<TestNote, TestCreateRequest>(
            new TestNoteFactory(), _repo.Object, _eventBus.Object);

        var id = await handler.HandleAsync(new CreateNoteCommand<TestCreateRequest>(TestData.CreateRequest()));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.Is<TestNote>(n => n.Body == "First comment")), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is NoteCreatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_PassesExtensionField_AndMentionEvents()
    {
        var handler = new CreateNoteCommandHandler<TestNote, TestCreateRequest>(
            new TestNoteFactory(), _repo.Object, _eventBus.Object);

        var req = TestData.CreateRequest() with { Visibility = "private", Mentions = new[] { Guid.NewGuid() } };
        await handler.HandleAsync(new CreateNoteCommand<TestCreateRequest>(req));

        _repo.Verify(r => r.Add(It.Is<TestNote>(n => n.Visibility == "private")), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is UserMentionedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestNote?)null);
        var handler = new UpdateNoteCommandHandler<TestNote, TestUpdateRequest>(_repo.Object, _eventBus.Object);

        await Should.ThrowAsync<NoteValidationException>(() =>
            handler.HandleAsync(new UpdateNoteCommand<TestUpdateRequest>(
                Guid.NewGuid(), new TestUpdateRequest { Body = "x" })).AsTask());
    }

    [Fact]
    public async Task Update_EditsBody_SavesAndPublishes()
    {
        var note = TestData.NewNote();
        _repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>())).ReturnsAsync(note);
        var handler = new UpdateNoteCommandHandler<TestNote, TestUpdateRequest>(_repo.Object, _eventBus.Object);

        await handler.HandleAsync(new UpdateNoteCommand<TestUpdateRequest>(
            note.Id, new TestUpdateRequest { Body = "Edited" }));

        note.Body.ShouldBe("Edited");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is NoteUpdatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Pin_SetsPinnedAt_AndSaves()
    {
        var note = TestData.NewNote();
        _repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>())).ReturnsAsync(note);
        var handler = new PinNoteCommandHandler<TestNote>(_repo.Object);

        await handler.HandleAsync(new PinNoteCommand(note.Id));

        note.PinnedAt.ShouldNotBeNull();
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Remove_SoftDeletes_AndPublishesRemovedEvent()
    {
        var note = TestData.NewNote();
        _repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>())).ReturnsAsync(note);
        var handler = new RemoveNoteCommandHandler<TestNote>(_repo.Object, _eventBus.Object);

        await handler.HandleAsync(new RemoveNoteCommand(note.Id));

        note.RemovedAt.ShouldNotBeNull();
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is NoteRemovedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }
}

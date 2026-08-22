using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.Notes.Application.Exceptions;
using Cheetah.Modules.Notes.Application.Notes;
using Cheetah.Modules.Notes.DomainEvents;
using Cheetah.Modules.Notes.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Notes.Application.Tests;

public class NoteCommandHandlerTests
{
    private readonly Mock<IRepository<TestNote, Guid>> _repo = new();
    private readonly Mock<IEventBus> _bus = new();

    private CreateNoteCommandHandler<TestNote, TestCreateRequest> CreateHandler()
        => new(new TestNoteFactory(), _repo.Object, _bus.Object);

    [Fact]
    public async Task Create_AddsSaves_AndPublishesDomainEvents()
    {
        var request = TestData.CreateRequest();
        var id = await CreateHandler().HandleAsync(new CreateNoteCommand<TestCreateRequest>(request));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.IsAny<TestNote>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _bus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is NoteCreatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_Reply_ChecksParentExists()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestNote?)null);

        var request = TestData.CreateRequest() with { ParentNoteId = Guid.NewGuid() };

        await Should.ThrowAsync<NoteValidationException>(async () =>
            await CreateHandler().HandleAsync(new CreateNoteCommand<TestCreateRequest>(request)));

        _repo.Verify(r => r.Add(It.IsAny<TestNote>()), Times.Never);
    }

    [Fact]
    public async Task Create_Reply_RejectsNestedThread()
    {
        var root = TestData.NewNote();
        var reply = TestNote.Create(TestData.CreateRequest() with
        {
            EntityType = root.EntityType, EntityId = root.EntityId, ParentNoteId = root.Id
        });
        _repo.Setup(r => r.GetByIdAsync(reply.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reply);

        var request = TestData.CreateRequest() with
        {
            EntityType = root.EntityType, EntityId = root.EntityId, ParentNoteId = reply.Id
        };

        var ex = await Should.ThrowAsync<NoteValidationException>(async () =>
            await CreateHandler().HandleAsync(new CreateNoteCommand<TestCreateRequest>(request)));

        ex.Message.ShouldContain("threads are flat");
    }

    [Fact]
    public async Task Create_Reply_RejectsParentFromAnotherEntity()
    {
        var parent = TestData.NewNote();
        _repo.Setup(r => r.GetByIdAsync(parent.Id, It.IsAny<CancellationToken>())).ReturnsAsync(parent);

        var request = TestData.CreateRequest() with
        {
            EntityType = EntityRefKeys.Customer, EntityId = Guid.NewGuid(), ParentNoteId = parent.Id
        };

        await Should.ThrowAsync<NoteValidationException>(async () =>
            await CreateHandler().HandleAsync(new CreateNoteCommand<TestCreateRequest>(request)));
    }

    [Fact]
    public async Task Create_Reply_AcceptsRootOfSameEntity()
    {
        var parent = TestData.NewNote();
        _repo.Setup(r => r.GetByIdAsync(parent.Id, It.IsAny<CancellationToken>())).ReturnsAsync(parent);

        var request = TestData.CreateRequest() with
        {
            EntityType = parent.EntityType, EntityId = parent.EntityId, ParentNoteId = parent.Id
        };

        var id = await CreateHandler().HandleAsync(new CreateNoteCommand<TestCreateRequest>(request));

        id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Update_EditsBody_AndPublishesUpdatedEvent()
    {
        var note = TestData.NewNote();
        _repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>())).ReturnsAsync(note);

        var handler = new UpdateNoteCommandHandler<TestNote, TestUpdateRequest>(_repo.Object, _bus.Object);
        await handler.HandleAsync(new UpdateNoteCommand<TestUpdateRequest>(
            note.Id, new TestUpdateRequest { Id = note.Id, Body = "edited" }));

        note.Body.ShouldBe("edited");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _bus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is NoteUpdatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithoutMentions_KeepsThemIntact()
    {
        var mention = Guid.NewGuid();
        var note = TestNote.Create(TestData.CreateRequest() with { Mentions = new[] { mention } });
        _repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>())).ReturnsAsync(note);

        var handler = new UpdateNoteCommandHandler<TestNote, TestUpdateRequest>(_repo.Object, _bus.Object);
        await handler.HandleAsync(new UpdateNoteCommand<TestUpdateRequest>(
            note.Id, new TestUpdateRequest { Id = note.Id, Body = "fixed typo" }));

        note.Mentions.ShouldBe(new[] { mention });
    }

    [Fact]
    public async Task Update_WithEmptyMentions_ClearsThem()
    {
        var note = TestNote.Create(TestData.CreateRequest() with { Mentions = new[] { Guid.NewGuid() } });
        _repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>())).ReturnsAsync(note);

        var handler = new UpdateNoteCommandHandler<TestNote, TestUpdateRequest>(_repo.Object, _bus.Object);
        await handler.HandleAsync(new UpdateNoteCommand<TestUpdateRequest>(
            note.Id, new TestUpdateRequest
            {
                Id = note.Id, Body = "no mentions anymore", Mentions = Array.Empty<Guid>()
            }));

        note.Mentions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Update_ChangesAttachments()
    {
        var note = TestNote.Create(TestData.CreateRequest() with { AttachmentFileIds = new[] { Guid.NewGuid() } });
        var newFile = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>())).ReturnsAsync(note);

        var handler = new UpdateNoteCommandHandler<TestNote, TestUpdateRequest>(_repo.Object, _bus.Object);
        await handler.HandleAsync(new UpdateNoteCommand<TestUpdateRequest>(
            note.Id, new TestUpdateRequest
            {
                Id = note.Id, Body = note.Body, AttachmentFileIds = new[] { newFile }
            }));

        note.AttachmentFileIds.ShouldBe(new[] { newFile });
    }

    [Fact]
    public async Task Update_UnknownNote_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestNote?)null);

        var handler = new UpdateNoteCommandHandler<TestNote, TestUpdateRequest>(_repo.Object, _bus.Object);

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
            await handler.HandleAsync(new UpdateNoteCommand<TestUpdateRequest>(
                Guid.NewGuid(), new TestUpdateRequest { Body = "x" })));
    }

    [Fact]
    public async Task Pin_And_Unpin_ChangePinnedAt_AndPublishEvents()
    {
        var note = TestData.NewNote();
        _repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>())).ReturnsAsync(note);

        await new PinNoteCommandHandler<TestNote>(_repo.Object, _bus.Object)
            .HandleAsync(new PinNoteCommand(note.Id));
        note.PinnedAt.ShouldNotBeNull();
        _bus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is NotePinnedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);

        await new UnpinNoteCommandHandler<TestNote>(_repo.Object, _bus.Object)
            .HandleAsync(new UnpinNoteCommand(note.Id));
        note.PinnedAt.ShouldBeNull();
        _bus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is NoteUnpinnedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Pin_UnknownNote_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestNote?)null);

        var handler = new PinNoteCommandHandler<TestNote>(_repo.Object, _bus.Object);

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
            await handler.HandleAsync(new PinNoteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task Remove_SoftDeletes_AndPublishesRemovedEvent()
    {
        var note = TestData.NewNote();
        _repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>())).ReturnsAsync(note);

        var handler = new RemoveNoteCommandHandler<TestNote>(_repo.Object, _bus.Object);
        await handler.HandleAsync(new RemoveNoteCommand(note.Id));

        note.RemovedAt.ShouldNotBeNull();
        _bus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is NoteRemovedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Remove_UnknownNote_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestNote?)null);

        var handler = new RemoveNoteCommandHandler<TestNote>(_repo.Object, _bus.Object);

        await Should.ThrowAsync<EntityNotFoundException>(async () =>
            await handler.HandleAsync(new RemoveNoteCommand(Guid.NewGuid())));
    }
}

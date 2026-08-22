using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Notes.Application.Notes;
using Cheetah.Modules.Notes.Domain.Abstractions;
using Cheetah.Modules.Notes.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Notes.Application.Tests;

public class NoteQueryHandlerTests
{
    private readonly Mock<IRepository<TestNote, Guid>> _repo = new();
    private readonly Mock<INoteReader<TestNote>> _reader = new();

    [Fact]
    public async Task GetById_ProjectsNote()
    {
        var note = TestData.NewNote();
        _repo.Setup(r => r.GetByIdAsync(note.Id, It.IsAny<CancellationToken>())).ReturnsAsync(note);

        var handler = new GetNoteByIdQueryHandler<TestNote, TestNoteDto>(_repo.Object, new TestNoteProjector());
        var dto = await handler.HandleAsync(new GetNoteByIdQuery<TestNoteDto>(note.Id));

        dto.ShouldNotBeNull();
        dto!.Id.ShouldBe(note.Id);
        dto.Body.ShouldBe(note.Body);
    }

    [Fact]
    public async Task GetById_Unknown_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestNote?)null);

        var handler = new GetNoteByIdQueryHandler<TestNote, TestNoteDto>(_repo.Object, new TestNoteProjector());

        (await handler.HandleAsync(new GetNoteByIdQuery<TestNoteDto>(Guid.NewGuid()))).ShouldBeNull();
    }

    [Fact]
    public async Task GetByEntity_DelegatesPagingToReader_AndProjects()
    {
        var entityId = Guid.NewGuid();
        var note = TestData.NewNote();

        _reader.Setup(r => r.GetPageByEntityAsync(
                EntityRefKeys.Deal, entityId, false, 5, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { note });

        var handler = new GetNotesByEntityQueryHandler<TestNote, TestNoteDto>(_reader.Object, new TestNoteProjector());
        var page = await handler.HandleAsync(new GetNotesByEntityQuery<TestNoteDto>(
            EntityRefKeys.Deal, entityId, Skip: 5, Take: 10));

        page.Count.ShouldBe(1);
        page[0].Id.ShouldBe(note.Id);
        _reader.VerifyAll(); // сортировку и срез делает БД, не память процесса
    }

    [Fact]
    public async Task GetByEntity_PinnedOnly_IsPassedThrough()
    {
        _reader.Setup(r => r.GetPageByEntityAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), true, It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TestNote>());

        var handler = new GetNotesByEntityQueryHandler<TestNote, TestNoteDto>(_reader.Object, new TestNoteProjector());
        await handler.HandleAsync(new GetNotesByEntityQuery<TestNoteDto>(
            EntityRefKeys.Deal, Guid.NewGuid(), PinnedOnly: true));

        _reader.VerifyAll();
    }

    [Fact]
    public async Task GetByEntity_ClampsTakeToMaxPageSize_AndNegativeSkipToZero()
    {
        var captured = (skip: -1, take: -1);
        _reader.Setup(r => r.GetPageByEntityAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Guid, bool, int, int, CancellationToken>(
                (_, _, _, skip, take, _) => captured = (skip, take))
            .ReturnsAsync(Array.Empty<TestNote>());

        var handler = new GetNotesByEntityQueryHandler<TestNote, TestNoteDto>(_reader.Object, new TestNoteProjector());
        await handler.HandleAsync(new GetNotesByEntityQuery<TestNoteDto>(
            EntityRefKeys.Deal, Guid.NewGuid(), Skip: -10, Take: NotesConstants.MaxPageSize + 10));

        captured.skip.ShouldBe(0);
        captured.take.ShouldBe(NotesConstants.MaxPageSize);
    }

    [Fact]
    public async Task GetByEntity_ZeroTake_FallsBackToDefaultPageSize()
    {
        var capturedTake = -1;
        _reader.Setup(r => r.GetPageByEntityAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Guid, bool, int, int, CancellationToken>((_, _, _, _, take, _) => capturedTake = take)
            .ReturnsAsync(Array.Empty<TestNote>());

        var handler = new GetNotesByEntityQueryHandler<TestNote, TestNoteDto>(_reader.Object, new TestNoteProjector());
        await handler.HandleAsync(new GetNotesByEntityQuery<TestNoteDto>(EntityRefKeys.Deal, Guid.NewGuid(), Take: 0));

        capturedTake.ShouldBe(NotesConstants.DefaultPageSize);
    }

    [Fact]
    public async Task GetReplies_DelegatesPagingToReader()
    {
        var parentId = Guid.NewGuid();
        var reply = TestData.NewNote();

        _reader.Setup(r => r.GetRepliesPageAsync(parentId, 0, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { reply });

        var handler = new GetNoteRepliesQueryHandler<TestNote, TestNoteDto>(_reader.Object, new TestNoteProjector());
        var replies = await handler.HandleAsync(new GetNoteRepliesQuery<TestNoteDto>(parentId, Take: 25));

        replies.Count.ShouldBe(1);
        replies[0].Id.ShouldBe(reply.Id);
        _reader.VerifyAll();
    }

    [Fact]
    public async Task GetReplies_ClampsTake()
    {
        var capturedTake = -1;
        _reader.Setup(r => r.GetRepliesPageAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, int, int, CancellationToken>((_, _, take, _) => capturedTake = take)
            .ReturnsAsync(Array.Empty<TestNote>());

        var handler = new GetNoteRepliesQueryHandler<TestNote, TestNoteDto>(_reader.Object, new TestNoteProjector());
        await handler.HandleAsync(new GetNoteRepliesQuery<TestNoteDto>(
            Guid.NewGuid(), Take: NotesConstants.MaxPageSize * 2));

        capturedTake.ShouldBe(NotesConstants.MaxPageSize);
    }
}

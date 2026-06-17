using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.NotesTimeline.Application.Notes;
using Cheetah.Modules.NotesTimeline.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.NotesTimeline.Application.Tests;

public class NoteQueryHandlerTests
{
    private readonly Mock<IRepository<TestNote, Guid>> _repo = new();

    [Fact]
    public async Task GetByEntity_ProjectsAndPutsPinnedFirst()
    {
        var older = TestData.NewNote();
        older.CreatedAt = DateTimeOffset.UtcNow.AddHours(-2);

        var pinned = TestData.NewNote();
        pinned.CreatedAt = DateTimeOffset.UtcNow.AddHours(-1);
        pinned.Pin();

        _repo.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TestNote>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestNote> { older, pinned });

        var handler = new GetNotesByEntityQueryHandler<TestNote, TestNoteDto>(_repo.Object, new TestNoteProjector());
        var result = await handler.HandleAsync(new GetNotesByEntityQuery<TestNoteDto>(EntityRefKeys.Deal, Guid.NewGuid()));

        result.Count.ShouldBe(2);
        result[0].Id.ShouldBe(pinned.Id); // закреплённая — первой
        result[0].Visibility.ShouldBeNull(); // проекция доп. поля проходит
    }
}

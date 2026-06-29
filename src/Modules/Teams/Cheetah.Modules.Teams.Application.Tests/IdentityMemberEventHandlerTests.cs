using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Identity.DomainEvents;
using Cheetah.Modules.Teams.Application.EventHandlers.Identity;
using Cheetah.Modules.Teams.Domain.Abstractions;
using Cheetah.Modules.Teams.Domain.Entities;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Teams.Application.Tests;

/// <summary>
/// Реактивное поддержание реплики участников по событиям Identity (в дополнение к bulk-синку).
/// Идемпотентность та же: апсёрт по Id + сравнение по хэшу.
/// </summary>
public class IdentityMemberEventHandlerTests
{
    private readonly Mock<IRepository<TeamMember, Guid>> _repo = new();

    private void SetupExisting(TeamMember? member)
        => _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

    [Fact]
    public async Task UserCreated_adds_new_member()
    {
        SetupExisting(null);
        var id = Guid.NewGuid();

        await new UserCreatedMemberHandler(_repo.Object)
            .HandleAsync(new UserCreatedEvent(id, "alice", "alice@x.io"));

        _repo.Verify(r => r.Add(It.Is<TeamMember>(m => m.Id == id && m.Name == "alice")), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserCreated_is_idempotent_for_existing_member()
    {
        var id = Guid.NewGuid();
        SetupExisting(TeamMember.CreateFromDirectory(new UserDirectoryEntry(id, "alice")));

        await new UserCreatedMemberHandler(_repo.Object)
            .HandleAsync(new UserCreatedEvent(id, "alice", "alice@x.io"));

        _repo.Verify(r => r.Add(It.IsAny<TeamMember>()), Times.Never);
        _repo.Verify(r => r.Update(It.IsAny<TeamMember>()), Times.Never);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UserNameChanged_updates_existing_member()
    {
        var id = Guid.NewGuid();
        var existing = TeamMember.CreateFromDirectory(new UserDirectoryEntry(id, "alice"));
        SetupExisting(existing);

        await new UserNameChangedMemberHandler(_repo.Object)
            .HandleAsync(new UserNameChangedEvent(id, "alice", "alice.smith"));

        existing.Name.ShouldBe("alice.smith");
        _repo.Verify(r => r.Update(existing), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserNameChanged_recreates_missing_member()
    {
        SetupExisting(null);
        var id = Guid.NewGuid();

        await new UserNameChangedMemberHandler(_repo.Object)
            .HandleAsync(new UserNameChangedEvent(id, "alice", "alice.smith"));

        _repo.Verify(r => r.Add(It.Is<TeamMember>(m => m.Id == id && m.Name == "alice.smith")), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserDeleted_removes_member()
    {
        var id = Guid.NewGuid();
        var existing = TeamMember.CreateFromDirectory(new UserDirectoryEntry(id, "alice"));
        SetupExisting(existing);

        await new UserDeletedMemberHandler(_repo.Object)
            .HandleAsync(new UserDeletedEvent(id, "alice"));

        _repo.Verify(r => r.Delete(existing), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserDeleted_is_noop_when_member_absent()
    {
        SetupExisting(null);

        await new UserDeletedMemberHandler(_repo.Object)
            .HandleAsync(new UserDeletedEvent(Guid.NewGuid(), "alice"));

        _repo.Verify(r => r.Delete(It.IsAny<TeamMember>()), Times.Never);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

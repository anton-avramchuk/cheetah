using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Teams.Application.Members;
using Cheetah.Modules.Teams.Domain.Abstractions;
using Cheetah.Modules.Teams.Domain.Entities;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Teams.Application.Tests;

public class TeamMemberDirectorySyncTests
{
    private readonly Mock<IIdentityUserDirectory> _directory = new();
    private readonly Mock<IRepository<TeamMember, Guid>> _repo = new();

    private TeamMemberDirectorySynchronizer Sut => new(_directory.Object, _repo.Object);

    private void SetupExisting(params TeamMember[] members)
        => _repo.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TeamMember>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(members.ToList());

    private void SetupDirectory(params UserDirectoryEntry[] entries)
        => _directory.Setup(d => d.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

    [Fact]
    public async Task New_users_are_added()
    {
        SetupDirectory(new UserDirectoryEntry(Guid.NewGuid(), "alice"), new UserDirectoryEntry(Guid.NewGuid(), "bob"));
        SetupExisting();

        var changed = await Sut.SyncAsync();

        changed.ShouldBe(2);
        _repo.Verify(r => r.Add(It.IsAny<TeamMember>()), Times.Exactly(2));
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Unchanged_users_are_not_written()
    {
        var id = Guid.NewGuid();
        var entry = new UserDirectoryEntry(id, "alice");
        SetupDirectory(entry);
        SetupExisting(TeamMember.CreateFromDirectory(entry)); // уже актуальна (тот же хэш)

        var changed = await Sut.SyncAsync();

        changed.ShouldBe(0);
        _repo.Verify(r => r.Update(It.IsAny<TeamMember>()), Times.Never);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); // БД не трогаем
    }

    [Fact]
    public async Task Changed_users_are_updated_only()
    {
        var id = Guid.NewGuid();
        var existing = TeamMember.CreateFromDirectory(new UserDirectoryEntry(id, "alice"));
        SetupExisting(existing);
        SetupDirectory(new UserDirectoryEntry(id, "alice.smith")); // имя изменилось

        var changed = await Sut.SyncAsync();

        changed.ShouldBe(1);
        existing.Name.ShouldBe("alice.smith");
        _repo.Verify(r => r.Update(existing), Times.Once);
        _repo.Verify(r => r.Add(It.IsAny<TeamMember>()), Times.Never);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

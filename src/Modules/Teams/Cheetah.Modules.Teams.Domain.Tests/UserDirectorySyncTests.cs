using Cheetah.Modules.Teams.Domain.Abstractions;
using Cheetah.Modules.Teams.Domain.Entities;
using Shouldly;

namespace Cheetah.Modules.Teams.Domain.Tests;

public class UserDirectorySyncTests
{
    [Fact]
    public void ComputeHash_is_stable_for_same_content()
    {
        var a = new UserDirectoryEntry(Guid.NewGuid(), "johndoe");
        var b = new UserDirectoryEntry(Guid.NewGuid(), "johndoe");

        a.ComputeHash().ShouldBe(b.ComputeHash()); // хэш по содержимому, без Id
    }

    [Fact]
    public void ComputeHash_differs_when_content_changes()
    {
        var id = Guid.NewGuid();
        var before = new UserDirectoryEntry(id, "johndoe");
        var after = new UserDirectoryEntry(id, "john.doe");

        before.ComputeHash().ShouldNotBe(after.ComputeHash());
    }

    [Fact]
    public void CreateFromDirectory_copies_id_and_fixes_hash()
    {
        var entry = new UserDirectoryEntry(Guid.NewGuid(), "johndoe");

        var member = TeamMember.CreateFromDirectory(entry);

        member.Id.ShouldBe(entry.Id);     // идентификаторы совпадают с Identity
        member.UserId.ShouldBe(entry.Id);
        member.Name.ShouldBe("johndoe");
        member.SyncHash.ShouldBe(entry.ComputeHash());
    }

    [Fact]
    public void CreateFromDirectory_empty_id_throws()
        => Should.Throw<ArgumentException>(() => TeamMember.CreateFromDirectory(new UserDirectoryEntry(Guid.Empty, "x")));

    [Fact]
    public void Apply_unchanged_snapshot_returns_false_and_does_not_touch()
    {
        var entry = new UserDirectoryEntry(Guid.NewGuid(), "johndoe");
        var member = TeamMember.CreateFromDirectory(entry);

        var changed = member.Apply(entry); // тот же снимок → реплику не трогаем

        changed.ShouldBeFalse();
    }

    [Fact]
    public void Apply_changed_snapshot_updates_and_returns_true()
    {
        var id = Guid.NewGuid();
        var member = TeamMember.CreateFromDirectory(new UserDirectoryEntry(id, "johndoe"));

        var changed = member.Apply(new UserDirectoryEntry(id, "john.doe"));

        changed.ShouldBeTrue();
        member.Name.ShouldBe("john.doe");
    }
}

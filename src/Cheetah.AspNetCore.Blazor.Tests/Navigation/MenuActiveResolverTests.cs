using Cheetah.AspNetCore.Blazor.Navigation;

namespace Cheetah.AspNetCore.Blazor.Tests.Navigation;

public class MenuActiveResolverTests
{
    private static readonly string?[] Urls = ["/teams", "/teams/roles", "/", null];

    [Theory]
    [InlineData("teams", "teams")]                 // точное совпадение
    [InlineData("teams/123", "teams")]             // деталь сущности → активен родитель /teams
    [InlineData("teams/123/edit", "teams")]        // вложенный путь сущности → тоже /teams
    [InlineData("teams/roles", "teams/roles")]     // самый специфичный выигрывает (не /teams)
    [InlineData("teams/roles/42", "teams/roles")]  // вложенность под /teams/roles
    public void Resolve_PicksMostSpecificMatch(string path, string expected)
        => MenuActiveResolver.Resolve(Urls, path).ShouldBe(expected);

    [Fact]
    public void Resolve_Home_MatchesOnlyEmptyPath()
    {
        MenuActiveResolver.Resolve(Urls, "").ShouldBe("");      // домашняя
        MenuActiveResolver.Resolve(Urls, "teams").ShouldBe("teams"); // не дом
    }

    [Fact]
    public void Resolve_NoMatch_ReturnsNull()
        => MenuActiveResolver.Resolve(["/teams"], "customers").ShouldBeNull();

    [Fact]
    public void Resolve_RespectsSegmentBoundary()
        // "/team" не должен матчить путь "teams" (совпадение только по границе сегмента)
        => MenuActiveResolver.Resolve(["/team"], "teams").ShouldBeNull();

    [Fact]
    public void Resolve_IgnoresQueryAndLeadingSlashAndCase()
        => MenuActiveResolver.Resolve(["/Teams"], "/teams/123?tab=info").ShouldBe("teams");
}

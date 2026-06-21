using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;

namespace Cheetah.AspNetCore.Blazor.Tests.Navigation;

public class MenuContributorProviderTests
{
    private sealed class StubContributor(string targetMenuId) : IMenuContributor
    {
        public string TargetMenuId { get; } = targetMenuId;
        public Task ConfigureMenuAsync(MenuBuilder builder) => Task.CompletedTask;
    }

    [Fact]
    public void GetContributors_ReturnsOnlyThoseMatchingMenuId()
    {
        var main1 = new StubContributor("main");
        var main2 = new StubContributor("main");
        var other = new StubContributor("other");
        var provider = new MenuContributorProvider([main1, main2, other]);

        var result = provider.GetContributors("main").ToList();

        result.Count.ShouldBe(2);
        result.ShouldContain(main1);
        result.ShouldContain(main2);
        result.ShouldNotContain(other);
    }

    [Fact]
    public void GetContributors_ReturnsEmpty_WhenNoneMatch()
    {
        var provider = new MenuContributorProvider([new StubContributor("main")]);

        provider.GetContributors("missing").ShouldBeEmpty();
    }
}

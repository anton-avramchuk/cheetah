using Cheetah.Expressions;
using Cheetah.Expressions.JsonLogic;
using Moq;
using Shouldly;

namespace Cheetah.Expressions.JsonLogic.Tests;

public class ExpressionPreprocessorTests
{
    // ---------- now ----------

    [Fact]
    public void RewriteSync_Replaces_Now_With_Iso_Timestamp()
    {
        var rewritten = ExpressionPreprocessor.RewriteSync("""{"now":[]}""");
        var asJson = System.Text.Json.Nodes.JsonNode.Parse(rewritten)!.GetValue<string>();
        var parsed = DateTimeOffset.Parse(asJson);
        parsed.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-5), DateTimeOffset.UtcNow.AddSeconds(5));
    }

    // ---------- regex ----------

    [Theory]
    [InlineData("""{"regex":["^\\d{3}-\\d{4}$","123-4567"]}""", "true")]
    [InlineData("""{"regex":["^\\d{3}-\\d{4}$","invalid"]}""", "false")]
    [InlineData("""{"regex":["[invalid","x"]}""", "false")]
    public void RewriteSync_Resolves_Regex_With_Literals(string input, string expected)
    {
        var rewritten = ExpressionPreprocessor.RewriteSync(input);
        rewritten.ShouldBe(expected);
    }

    [Fact]
    public void RewriteSync_Regex_With_Var_Uses_Context()
    {
        var ctx = new Dictionary<string, object?> { ["phone"] = "555-1234" };
        var rewritten = ExpressionPreprocessor.RewriteSync(
            """{"regex":["^\\d{3}-\\d{4}$",{"var":"phone"}]}""", ctx);
        rewritten.ShouldBe("true");
    }

    [Fact]
    public void RewriteSync_Regex_Var_Missing_From_Context_Returns_False()
    {
        var rewritten = ExpressionPreprocessor.RewriteSync(
            """{"regex":["^\\d+$",{"var":"absent"}]}""",
            new Dictionary<string, object?>());
        rewritten.ShouldBe("false");
    }

    // ---------- reference_exists (async) ----------

    [Fact]
    public async Task ResolveReferences_True_When_Lookup_Says_Exists()
    {
        var lookup = new Mock<IReferenceLookup>();
        lookup.Setup(l => l.ExistsAsync("Cities", "Moscow", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var resolved = await ExpressionPreprocessor.ResolveReferencesAsync(
            """{"reference_exists":["Cities","Moscow"]}""", lookup.Object);
        resolved.ShouldBe("true");
    }

    [Fact]
    public async Task ResolveReferences_Without_Lookup_Returns_False()
    {
        var resolved = await ExpressionPreprocessor.ResolveReferencesAsync(
            """{"reference_exists":["Cities","Moscow"]}""", lookup: null);
        resolved.ShouldBe("false");
    }

    [Fact]
    public async Task ResolveReferences_Inside_And_Operator()
    {
        var lookup = new Mock<IReferenceLookup>();
        lookup.Setup(l => l.ExistsAsync("Cities", "Moscow", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var resolved = await ExpressionPreprocessor.ResolveReferencesAsync(
            """{"and":[{"reference_exists":["Cities","Moscow"]},{">":[1,0]}]}""", lookup.Object);

        resolved.ShouldContain("true");
        resolved.ShouldContain("1");
    }

    [Fact]
    public async Task ResolveReferences_Var_From_Context()
    {
        var lookup = new Mock<IReferenceLookup>();
        lookup.Setup(l => l.ExistsAsync("Cities", "Moscow", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var resolved = await ExpressionPreprocessor.ResolveReferencesAsync(
            """{"reference_exists":["Cities",{"var":"document.cityId"}]}""",
            lookup.Object,
            new Dictionary<string, object?>
            {
                ["document"] = new Dictionary<string, object?> { ["cityId"] = "Moscow" }
            });
        resolved.ShouldBe("true");
        lookup.Verify(l => l.ExistsAsync("Cities", "Moscow", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveReferences_Var_Without_Context_Returns_False()
    {
        var lookup = new Mock<IReferenceLookup>();
        var resolved = await ExpressionPreprocessor.ResolveReferencesAsync(
            """{"reference_exists":["Cities",{"var":"missing"}]}""", lookup.Object);
        resolved.ShouldBe("false");
        lookup.Verify(l => l.ExistsAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

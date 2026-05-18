using Cheetah.Expressions;
using Shouldly;

namespace Cheetah.Expressions.Tests;

public class DictionaryExpressionContextTests
{
    [Fact]
    public void TryGet_FlatKey_Returns_Value()
    {
        var ctx = new DictionaryExpressionContext(new Dictionary<string, object?>
        {
            ["status"] = "Draft"
        });

        ctx.TryGet("status", out var value).ShouldBeTrue();
        value.ShouldBe("Draft");
    }

    [Fact]
    public void TryGet_DottedPath_Walks_Nested_Dictionaries()
    {
        var ctx = new DictionaryExpressionContext(new Dictionary<string, object?>
        {
            ["document"] = new Dictionary<string, object?>
            {
                ["amount"] = 100000m,
                ["counterparty"] = new Dictionary<string, object?>
                {
                    ["type"] = "VIP"
                }
            }
        });

        ctx.TryGet("document.amount", out var amount).ShouldBeTrue();
        amount.ShouldBe(100000m);

        ctx.TryGet("document.counterparty.type", out var type).ShouldBeTrue();
        type.ShouldBe("VIP");
    }

    [Fact]
    public void TryGet_MissingKey_Returns_False()
    {
        var ctx = new DictionaryExpressionContext(new Dictionary<string, object?>());
        ctx.TryGet("missing", out var value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void TryGet_DottedPath_Stops_On_NonDictionary_Segment()
    {
        var ctx = new DictionaryExpressionContext(new Dictionary<string, object?>
        {
            ["amount"] = 100
        });

        // amount — это число, "amount.value" не существует.
        ctx.TryGet("amount.value", out var value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void TryGet_NullSegment_Returns_True_With_Null_Value()
    {
        var ctx = new DictionaryExpressionContext(new Dictionary<string, object?>
        {
            ["status"] = null
        });

        ctx.TryGet("status", out var value).ShouldBeTrue();
        value.ShouldBeNull();
    }

    [Fact]
    public void TryGet_EmptyPath_Returns_False()
    {
        var ctx = new DictionaryExpressionContext(new Dictionary<string, object?>());
        ctx.TryGet("", out var value).ShouldBeFalse();
        value.ShouldBeNull();
    }
}

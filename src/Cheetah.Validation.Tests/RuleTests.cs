using Cheetah.Validation.Rules;
using Shouldly;

namespace Cheetah.Validation.Tests;

public class RuleTests
{
    // ---------- RequiredRule ----------

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("ok", true)]
    [InlineData(0, true)]
    [InlineData(false, true)]
    public async Task RequiredRule(object? value, bool expectedValid)
    {
        var rule = new RequiredRule();
        var outcome = await rule.ValidateAsync(TestHelpers.MakeContext("field", value));
        outcome.IsValid.ShouldBe(expectedValid);
    }

    // ---------- StringLengthRule ----------

    [Theory]
    [InlineData("abc", 2, 5, true)]
    [InlineData("a", 2, 5, false)]
    [InlineData("abcdef", 2, 5, false)]
    [InlineData("abc", null, 5, true)]
    [InlineData("abc", 2, null, true)]
    public async Task StringLengthRule_With_Bounds(string s, int? min, int? max, bool expected)
    {
        var rule = new StringLengthRule { Min = min, Max = max };
        var outcome = await rule.ValidateAsync(TestHelpers.MakeContext("f", s));
        outcome.IsValid.ShouldBe(expected);
    }

    [Fact]
    public async Task StringLengthRule_Null_Is_Valid()
    {
        var rule = new StringLengthRule { Min = 5 };
        var outcome = await rule.ValidateAsync(TestHelpers.MakeContext("f", null));
        outcome.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task StringLengthRule_Non_String_Is_Invalid()
    {
        var rule = new StringLengthRule { Min = 1 };
        var outcome = await rule.ValidateAsync(TestHelpers.MakeContext("f", 42));
        outcome.IsValid.ShouldBeFalse();
        outcome.Code.ShouldBe("string_length.not_string");
    }

    // ---------- NumberRangeRule ----------

    [Theory]
    [InlineData(5, 1, 10, true)]
    [InlineData(0, 1, 10, false)]
    [InlineData(11, 1, 10, false)]
    [InlineData(5, 5, 5, true)] // граница
    public async Task NumberRangeRule_Int(int v, int min, int max, bool expected)
    {
        var rule = new NumberRangeRule { Min = min, Max = max };
        var outcome = await rule.ValidateAsync(TestHelpers.MakeContext("f", v));
        outcome.IsValid.ShouldBe(expected);
    }

    [Fact]
    public async Task NumberRangeRule_Accepts_Decimal_And_String_Number()
    {
        var rule = new NumberRangeRule { Min = 0m, Max = 100m };
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", 42.5m))).IsValid.ShouldBeTrue();
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", "42.5"))).IsValid.ShouldBeTrue();
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", "abc"))).IsValid.ShouldBeFalse();
    }

    // ---------- DateRangeRule ----------

    [Fact]
    public async Task DateRangeRule_Within_Bounds_Is_Valid()
    {
        var rule = new DateRangeRule
        {
            Min = DateTimeOffset.Parse("2020-01-01Z"),
            Max = DateTimeOffset.Parse("2030-01-01Z")
        };
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", DateTimeOffset.Parse("2025-06-15Z")))).IsValid.ShouldBeTrue();
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", DateTimeOffset.Parse("2019-01-01Z")))).IsValid.ShouldBeFalse();
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", DateTimeOffset.Parse("2031-01-01Z")))).IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task DateRangeRule_Accepts_String_Iso()
    {
        var rule = new DateRangeRule { Min = DateTimeOffset.Parse("2020-01-01Z") };
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", "2025-06-15T00:00:00Z"))).IsValid.ShouldBeTrue();
    }

    // ---------- PatternRule ----------

    [Theory]
    [InlineData("^\\d{3}-\\d{4}$", "123-4567", true)]
    [InlineData("^\\d{3}-\\d{4}$", "abc", false)]
    [InlineData("[invalid", "x", false)] // bad regex → fail
    public async Task PatternRule(string pattern, string input, bool expected)
    {
        var rule = new PatternRule { Pattern = pattern };
        var outcome = await rule.ValidateAsync(TestHelpers.MakeContext("f", input));
        outcome.IsValid.ShouldBe(expected);
    }

    [Fact]
    public async Task PatternRule_Null_Is_Valid()
    {
        var rule = new PatternRule { Pattern = "^.+$" };
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", null))).IsValid.ShouldBeTrue();
    }

    // ---------- EnumValueRule ----------

    [Fact]
    public async Task EnumValueRule()
    {
        var rule = new EnumValueRule { AllowedValues = new[] { "Draft", "Published", "Archived" } };
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", "Draft"))).IsValid.ShouldBeTrue();
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", "Unknown"))).IsValid.ShouldBeFalse();
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", null))).IsValid.ShouldBeTrue();
    }

    // ---------- CompareRule ----------

    [Fact]
    public async Task CompareRule_EndDate_Gt_StartDate()
    {
        var rule = new CompareRule { OtherFieldPath = "startDate", Operator = CompareOperator.Gt };
        var allValues = new Dictionary<string, object?>
        {
            ["startDate"] = DateTimeOffset.Parse("2025-01-01Z")
        };

        var ok = await rule.ValidateAsync(TestHelpers.MakeContext("endDate",
            DateTimeOffset.Parse("2025-06-01Z"), allValues));
        ok.IsValid.ShouldBeTrue();

        var bad = await rule.ValidateAsync(TestHelpers.MakeContext("endDate",
            DateTimeOffset.Parse("2024-01-01Z"), allValues));
        bad.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task CompareRule_Both_Null_Is_Valid()
    {
        var rule = new CompareRule { OtherFieldPath = "other", Operator = CompareOperator.Eq };
        var outcome = await rule.ValidateAsync(TestHelpers.MakeContext("f", null,
            new Dictionary<string, object?> { ["other"] = null }));
        outcome.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task CompareRule_Type_Mismatch_Is_Invalid()
    {
        var rule = new CompareRule { OtherFieldPath = "other", Operator = CompareOperator.Eq };
        var outcome = await rule.ValidateAsync(TestHelpers.MakeContext("f", 10,
            new Dictionary<string, object?> { ["other"] = "abc" }));
        outcome.IsValid.ShouldBeFalse();
    }

    // ---------- ExpressionRule ----------

    [Fact]
    public async Task ExpressionRule_With_JsonLogic_Evaluator()
    {
        var rule = new ExpressionRule
        {
            Expression = """{">":[{"var":"value"},100]}""",
            Message = "amount must exceed 100"
        };
        var sp = TestHelpers.WithJsonLogicEvaluator();

        var ok = await rule.ValidateAsync(TestHelpers.MakeContext("amount", 150,
            new Dictionary<string, object?> { ["amount"] = 150 }, sp));
        ok.IsValid.ShouldBeTrue();

        var bad = await rule.ValidateAsync(TestHelpers.MakeContext("amount", 50,
            new Dictionary<string, object?> { ["amount"] = 50 }, sp));
        bad.IsValid.ShouldBeFalse();
        bad.Message.ShouldBe("amount must exceed 100");
    }

    [Fact]
    public async Task ExpressionRule_Without_Evaluator_Is_NoOp()
    {
        var rule = new ExpressionRule { Expression = """{"==":[1,2]}""" };
        var outcome = await rule.ValidateAsync(TestHelpers.MakeContext("f", null));
        outcome.IsValid.ShouldBeTrue();
    }

    // ---------- RequiredIfRule ----------

    [Fact]
    public async Task RequiredIfRule_When_Condition_True_Requires_Value()
    {
        var rule = new RequiredIfRule { Expression = """{"==":[{"var":"status"},"Signed"]}""" };
        var sp = TestHelpers.WithJsonLogicEvaluator();

        var ctxWithSigned = new Dictionary<string, object?> { ["status"] = "Signed" };
        var bad = await rule.ValidateAsync(TestHelpers.MakeContext("signedAt", null, ctxWithSigned, sp));
        bad.IsValid.ShouldBeFalse();

        var ok = await rule.ValidateAsync(TestHelpers.MakeContext("signedAt", "2025-01-01", ctxWithSigned, sp));
        ok.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task RequiredIfRule_When_Condition_False_Allows_Null()
    {
        var rule = new RequiredIfRule { Expression = """{"==":[{"var":"status"},"Signed"]}""" };
        var sp = TestHelpers.WithJsonLogicEvaluator();

        var ctxDraft = new Dictionary<string, object?> { ["status"] = "Draft" };
        var ok = await rule.ValidateAsync(TestHelpers.MakeContext("signedAt", null, ctxDraft, sp));
        ok.IsValid.ShouldBeTrue();
    }

    // ---------- UniqueRule (placeholder) ----------

    [Fact]
    public async Task UniqueRule_Is_Noop_In_MVP()
    {
        var rule = new UniqueRule { Scope = "Documents.Contract.Number" };
        (await rule.ValidateAsync(TestHelpers.MakeContext("f", "anything"))).IsValid.ShouldBeTrue();
    }
}

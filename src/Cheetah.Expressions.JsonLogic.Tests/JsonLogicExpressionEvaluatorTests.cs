using Cheetah.Expressions;
using Cheetah.Expressions.JsonLogic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Cheetah.Expressions.JsonLogic.Tests;

public class JsonLogicExpressionEvaluatorTests
{
    private readonly IExpressionEvaluator _sut = CreateEvaluator();

    private static JsonLogicExpressionEvaluator CreateEvaluator(ExpressionOptions? options = null)
        => new(Options.Create(options ?? new ExpressionOptions()),
               NullLogger<JsonLogicExpressionEvaluator>.Instance);

    // ---------- Базовые операторы сравнения ----------

    [Theory]
    [InlineData("""{"==":[1,1]}""", true)]
    [InlineData("""{"==":[1,2]}""", false)]
    [InlineData("""{"!=":[1,2]}""", true)]
    [InlineData("""{">":[5,3]}""", true)]
    [InlineData("""{"<":[5,3]}""", false)]
    [InlineData("""{">=":[3,3]}""", true)]
    [InlineData("""{"<=":[3,3]}""", true)]
    public async Task Compare_Operators(string expr, bool expected)
    {
        var ok = await _sut.EvaluateBooleanAsync(expr, new Dictionary<string, object?>());
        ok.ShouldBe(expected);
    }

    [Theory]
    [InlineData("""{"and":[true,true]}""", true)]
    [InlineData("""{"and":[true,false]}""", false)]
    [InlineData("""{"or":[false,true]}""", true)]
    [InlineData("""{"or":[false,false]}""", false)]
    [InlineData("""{"!":true}""", false)]
    [InlineData("""{"!":false}""", true)]
    public async Task Boolean_Operators(string expr, bool expected)
    {
        var ok = await _sut.EvaluateBooleanAsync(expr, new Dictionary<string, object?>());
        ok.ShouldBe(expected);
    }

    // ---------- Арифметика ----------

    [Theory]
    [InlineData("""{"+":[2,3]}""", 5.0)]
    [InlineData("""{"-":[10,4]}""", 6.0)]
    [InlineData("""{"*":[3,4]}""", 12.0)]
    [InlineData("""{"/":[10,2]}""", 5.0)]
    [InlineData("""{"%":[10,3]}""", 1.0)]
    public async Task Arithmetic(string expr, double expected)
    {
        var result = await _sut.EvaluateAsync<double>(expr, new Dictionary<string, object?>());
        result.Success.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    // ---------- var и dotted-paths ----------

    [Fact]
    public async Task Var_Resolves_Nested_Field()
    {
        var ctx = new Dictionary<string, object?>
        {
            ["document"] = new Dictionary<string, object?> { ["amount"] = 100000 }
        };

        var expr = """{">":[{"var":"document.amount"},50000]}""";
        var ok = await _sut.EvaluateBooleanAsync(expr, ctx);
        ok.ShouldBeTrue();
    }

    [Fact]
    public async Task Combined_And_With_Var()
    {
        var ctx = new Dictionary<string, object?>
        {
            ["amount"] = 150000,
            ["category"] = "VIP"
        };

        var expr = """{"and":[{">":[{"var":"amount"},100000]},{"==":[{"var":"category"},"VIP"]}]}""";
        var ok = await _sut.EvaluateBooleanAsync(expr, ctx);
        ok.ShouldBeTrue();
    }

    // ---------- null-safety ----------

    [Fact]
    public async Task Var_Of_Missing_Field_Does_Not_Crash()
    {
        var expr = """{"==":[{"var":"missing"},null]}""";
        var ok = await _sut.EvaluateBooleanAsync(expr, new Dictionary<string, object?>());
        ok.ShouldBeTrue();
    }

    // ---------- Кастомные операторы ----------

    [Fact]
    public async Task Now_Returns_Current_Time_String()
    {
        var expr = """{"now":[]}""";
        var result = await _sut.EvaluateAsync<string>(expr, new Dictionary<string, object?>());
        result.Success.ShouldBeTrue(customMessage: $"Error: {result.Error}");
        DateTimeOffset.Parse(result.Value!).ShouldBeInRange(
            DateTimeOffset.UtcNow.AddSeconds(-5),
            DateTimeOffset.UtcNow.AddSeconds(5));
    }

    [Theory]
    [InlineData("""{"regex":["^\\d{3}-\\d{4}$","123-4567"]}""", true)]
    [InlineData("""{"regex":["^\\d{3}-\\d{4}$","invalid"]}""", false)]
    [InlineData("""{"regex":["[invalid","x"]}""", false)] // невалидный pattern → false
    public async Task Regex_Operator(string expr, bool expected)
    {
        var ok = await _sut.EvaluateBooleanAsync(expr, new Dictionary<string, object?>());
        ok.ShouldBe(expected);
    }

    [Fact]
    public async Task Regex_With_Var()
    {
        var ctx = new Dictionary<string, object?> { ["phone"] = "555-1234" };
        var expr = """{"regex":["^\\d{3}-\\d{4}$",{"var":"phone"}]}""";
        var ok = await _sut.EvaluateBooleanAsync(expr, ctx);
        ok.ShouldBeTrue();
    }

    // ---------- Лимиты ----------

    [Fact]
    public async Task MaxExpressionLength_Returns_Failure()
    {
        var sut = CreateEvaluator(new ExpressionOptions { MaxExpressionLength = 10 });
        var expr = """{"==":[1,1]}"""; // длиннее 10
        var result = await sut.EvaluateAsync<bool>(expr, new Dictionary<string, object?>());
        result.Success.ShouldBeFalse();
        result.Error!.ShouldContain("MaxExpressionLength");
    }

    [Fact]
    public async Task MaxNestingDepth_Returns_Failure()
    {
        var sut = CreateEvaluator(new ExpressionOptions { MaxNestingDepth = 2 });
        // Глубина 4: {"!": {"!": {"!": {"!":true} } } }
        var expr = """{"!":{"!":{"!":{"!":true}}}}""";
        var result = await sut.EvaluateAsync<bool>(expr, new Dictionary<string, object?>());
        result.Success.ShouldBeFalse();
        result.Error!.ShouldContain("MaxNestingDepth");
    }

    // ---------- Parse ----------

    [Fact]
    public void Parse_Valid_Returns_Ok_With_Variables()
    {
        var expr = """{"and":[{">":[{"var":"document.amount"},100]},{"==":[{"var":"user.role"},"admin"]}]}""";
        var parsed = _sut.Parse(expr);
        parsed.Valid.ShouldBeTrue();
        parsed.ReferencedVariables.ShouldBe(new[] { "document.amount", "user.role" });
    }

    [Fact]
    public void Parse_Invalid_Json_Returns_Failure()
    {
        var parsed = _sut.Parse("{not valid");
        parsed.Valid.ShouldBeFalse();
        parsed.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Parse_Var_Array_Form()
    {
        // {"var":["foo", "default"]} — JsonLogic поддерживает форму с default-значением.
        var expr = """{"==":[{"var":["foo","default"]},"x"]}""";
        var parsed = _sut.Parse(expr);
        parsed.Valid.ShouldBeTrue();
        parsed.ReferencedVariables.ShouldContain("foo");
    }

    // ---------- EvaluateBooleanAsync поведение ----------

    [Fact]
    public async Task EvaluateBooleanAsync_Returns_False_For_Invalid_Expression()
    {
        var ok = await _sut.EvaluateBooleanAsync("{not valid", new Dictionary<string, object?>());
        ok.ShouldBeFalse();
    }

    [Fact]
    public async Task EvaluateBooleanAsync_Returns_False_For_NonBoolean_Result()
    {
        var ok = await _sut.EvaluateBooleanAsync("""{"+":[1,2]}""", new Dictionary<string, object?>());
        ok.ShouldBeFalse();
    }
}

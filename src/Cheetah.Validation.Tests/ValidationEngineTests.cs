using Cheetah.Validation.Rules;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cheetah.Validation.Tests;

public class ValidationEngineTests
{
    [Fact]
    public async Task Aggregates_All_Errors_Across_Fields()
    {
        var engine = new ValidationEngine();
        var fields = new List<FieldValidation>
        {
            new("name", null, new IValidationRule[] { new RequiredRule() }),
            new("age",  3,    new IValidationRule[] { new NumberRangeRule { Min = 18 } }),
            new("zip",  "x",  new IValidationRule[] { new PatternRule { Pattern = "^\\d{5}$" } })
        };

        var result = await engine.ValidateAsync(fields, new Dictionary<string, object?>(),
            TestHelpers.EmptyServices);

        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(3);
        result.Errors.Select(e => e.FieldPath).ShouldBe(new[] { "name", "age", "zip" });
    }

    [Fact]
    public async Task All_Valid_Returns_Valid_Result()
    {
        var engine = new ValidationEngine();
        var fields = new List<FieldValidation>
        {
            new("name", "John", new IValidationRule[] { new RequiredRule(), new StringLengthRule { Min = 1, Max = 100 } }),
            new("age",  25,     new IValidationRule[] { new NumberRangeRule { Min = 18, Max = 99 } })
        };

        var result = await engine.ValidateAsync(fields, new Dictionary<string, object?>(),
            TestHelpers.EmptyServices);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task Cross_Field_Rules_See_All_Values()
    {
        var engine = new ValidationEngine();
        var allValues = new Dictionary<string, object?>
        {
            ["start"] = DateTimeOffset.Parse("2025-01-01Z"),
            ["end"]   = DateTimeOffset.Parse("2024-06-01Z")
        };
        var fields = new List<FieldValidation>
        {
            new("end", allValues["end"],
                new IValidationRule[] { new CompareRule { OtherFieldPath = "start", Operator = CompareOperator.Gt } })
        };

        var result = await engine.ValidateAsync(fields, allValues, TestHelpers.EmptyServices);
        result.IsValid.ShouldBeFalse();
        result.Errors.Single().FieldPath.ShouldBe("end");
    }

    [Fact]
    public async Task Engine_Uses_Rule_Code_From_Outcome()
    {
        var engine = new ValidationEngine();
        var fields = new List<FieldValidation>
        {
            new("phone", "abc",
                new IValidationRule[] { new PatternRule { Pattern = "^\\d+$" } })
        };

        var result = await engine.ValidateAsync(fields, new Dictionary<string, object?>(),
            TestHelpers.EmptyServices);
        result.Errors.Single().Code.ShouldBe("pattern.mismatch");
    }
}

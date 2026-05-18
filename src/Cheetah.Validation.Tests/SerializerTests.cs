using System.Text.Json;
using Cheetah.Validation.Rules;
using Shouldly;

namespace Cheetah.Validation.Tests;

public class SerializerTests
{
    private readonly IValidationRuleSerializer _sut = new ValidationRuleSerializer();

    [Fact]
    public void Roundtrip_Required()
    {
        var json = _sut.Serialize(new RequiredRule());
        var back = _sut.Deserialize(json);
        back.ShouldBeOfType<RequiredRule>();
    }

    [Fact]
    public void Roundtrip_StringLength()
    {
        var rule = new StringLengthRule { Min = 3, Max = 100 };
        var json = _sut.Serialize(rule);
        var back = _sut.Deserialize(json).ShouldBeOfType<StringLengthRule>();
        back.Min.ShouldBe(3);
        back.Max.ShouldBe(100);
    }

    [Fact]
    public void Roundtrip_NumberRange_And_Pattern()
    {
        var rules = new IValidationRule[]
        {
            new NumberRangeRule { Min = 0, Max = 1000 },
            new PatternRule { Pattern = "^\\d+$" }
        };
        var json = _sut.SerializeMany(rules);
        var back = _sut.DeserializeMany(json);
        back.Count.ShouldBe(2);
        back[0].ShouldBeOfType<NumberRangeRule>();
        back[1].ShouldBeOfType<PatternRule>();
    }

    [Fact]
    public void Roundtrip_Compare_Preserves_Operator()
    {
        var rule = new CompareRule { OtherFieldPath = "startDate", Operator = CompareOperator.Gt };
        var json = _sut.Serialize(rule);
        var back = _sut.Deserialize(json).ShouldBeOfType<CompareRule>();
        back.OtherFieldPath.ShouldBe("startDate");
        back.Operator.ShouldBe(CompareOperator.Gt);
    }

    [Fact]
    public void Roundtrip_EnumValue()
    {
        var rule = new EnumValueRule { AllowedValues = new[] { "A", "B", "C" } };
        var json = _sut.Serialize(rule);
        var back = _sut.Deserialize(json).ShouldBeOfType<EnumValueRule>();
        back.AllowedValues.ShouldBe(new[] { "A", "B", "C" });
    }

    [Fact]
    public void Roundtrip_Expression_And_RequiredIf()
    {
        var rules = new IValidationRule[]
        {
            new ExpressionRule { Expression = """{">":[1,0]}""", Message = "must be positive" },
            new RequiredIfRule { Expression = """{"==":[{"var":"status"},"Signed"]}""" }
        };
        var json = _sut.SerializeMany(rules);
        var back = _sut.DeserializeMany(json);
        back[0].ShouldBeOfType<ExpressionRule>().Message.ShouldBe("must be positive");
        back[1].ShouldBeOfType<RequiredIfRule>().Expression.ShouldContain("status");
    }

    [Fact]
    public void Unknown_Type_Throws()
    {
        var json = """{"$type":"NotARealRule","x":1}""";
        Should.Throw<JsonException>(() => _sut.Deserialize(json));
    }

    [Fact]
    public void Serialized_Json_Contains_TypeDiscriminator()
    {
        var json = _sut.Serialize(new RequiredRule());
        json.ShouldContain("\"$type\"");
        json.ShouldContain("\"RequiredRule\"");
    }
}

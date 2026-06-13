using System.Linq.Expressions;
using Cheetah.Core.Dapper.Mapping;
using Cheetah.Core.Dapper.Specifications;
using Cheetah.Core.Specification;
using Shouldly;

namespace Cheetah.Core.Dapper.Tests;

public class ExpressionToSqlParserTests
{
    private readonly ExpressionToSqlParser _parser;

    public ExpressionToSqlParserTests()
    {
        var registry = new EntityMapRegistry([new ParserEntityMap()]);
        _parser = new ExpressionToSqlParser(registry, new TestDialect());
    }

    private SqlWhere Parse(Expression<Func<ParserEntity, bool>> expression)
        => _parser.Parse(new ExpressionSpecification<ParserEntity>(expression));

    [Fact]
    public void Equality_Produces_Equals_With_Parameter()
    {
        var result = Parse(x => x.Name == "John");

        result.Sql.ShouldBe("\"Name\" = @p0");
        result.Parameters["p0"].ShouldBe("John");
    }

    [Fact]
    public void Inequality_Produces_NotEquals()
    {
        var result = Parse(x => x.Age != 18);

        result.Sql.ShouldBe("\"Age\" <> @p0");
        result.Parameters["p0"].ShouldBe(18);
    }

    [Theory]
    [InlineData(">")]
    [InlineData(">=")]
    [InlineData("<")]
    [InlineData("<=")]
    public void Comparison_Operators_Are_Translated(string op)
    {
        var result = op switch
        {
            ">" => Parse(x => x.Age > 18),
            ">=" => Parse(x => x.Age >= 18),
            "<" => Parse(x => x.Age < 18),
            _ => Parse(x => x.Age <= 18),
        };

        result.Sql.ShouldBe($"\"Age\" {op} @p0");
        result.Parameters["p0"].ShouldBe(18);
    }

    [Fact]
    public void Reversed_Operand_Order_Flips_The_Operator()
    {
        // constant on the left: 18 < Age  =>  Age > 18
        var result = Parse(x => 18 < x.Age);

        result.Sql.ShouldBe("\"Age\" > @p0");
        result.Parameters["p0"].ShouldBe(18);
    }

    [Fact]
    public void AndAlso_Combines_With_Parentheses()
    {
        var result = Parse(x => x.Age > 18 && x.IsActive);

        result.Sql.ShouldBe("(\"Age\" > @p0 AND \"IsActive\" = @p1)");
        result.Parameters["p0"].ShouldBe(18);
        result.Parameters["p1"].ShouldBe(true);
    }

    [Fact]
    public void OrElse_Combines_With_Parentheses()
    {
        var result = Parse(x => x.Age < 18 || x.Name == "John");

        result.Sql.ShouldBe("(\"Age\" < @p0 OR \"Name\" = @p1)");
        result.Parameters["p0"].ShouldBe(18);
        result.Parameters["p1"].ShouldBe("John");
    }

    [Fact]
    public void Not_Negates_The_Inner_Predicate()
    {
        var result = Parse(x => !x.IsActive);

        result.Sql.ShouldBe("NOT (\"IsActive\" = @p0)");
        result.Parameters["p0"].ShouldBe(true);
    }

    [Fact]
    public void Boolean_Member_Becomes_Equals_True()
    {
        var result = Parse(x => x.IsActive);

        result.Sql.ShouldBe("\"IsActive\" = @p0");
        result.Parameters["p0"].ShouldBe(true);
    }

    [Fact]
    public void Equality_With_Null_Becomes_Is_Null()
    {
        var result = Parse(x => x.Nickname == null);

        result.Sql.ShouldBe("\"nick_name\" IS NULL");
        result.Parameters.ShouldBeEmpty();
    }

    [Fact]
    public void Inequality_With_Null_Becomes_Is_Not_Null()
    {
        var result = Parse(x => x.Nickname != null);

        result.Sql.ShouldBe("\"nick_name\" IS NOT NULL");
        result.Parameters.ShouldBeEmpty();
    }

    [Fact]
    public void Captured_Variable_Is_Evaluated_To_A_Parameter()
    {
        var expected = "Captured";
        var result = Parse(x => x.Name == expected);

        result.Sql.ShouldBe("\"Name\" = @p0");
        result.Parameters["p0"].ShouldBe("Captured");
    }

    [Fact]
    public void Contains_Becomes_Like_With_Wildcards_On_Both_Sides()
    {
        var result = Parse(x => x.Name.Contains("oh"));

        result.Sql.ShouldBe("\"Name\" LIKE @p0");
        result.Parameters["p0"].ShouldBe("%oh%");
    }

    [Fact]
    public void StartsWith_Becomes_Like_With_Trailing_Wildcard()
    {
        var result = Parse(x => x.Name.StartsWith("Jo"));

        result.Sql.ShouldBe("\"Name\" LIKE @p0");
        result.Parameters["p0"].ShouldBe("Jo%");
    }

    [Fact]
    public void EndsWith_Becomes_Like_With_Leading_Wildcard()
    {
        var result = Parse(x => x.Name.EndsWith("hn"));

        result.Sql.ShouldBe("\"Name\" LIKE @p0");
        result.Parameters["p0"].ShouldBe("%hn");
    }

    [Fact]
    public void Explicit_Column_Name_Override_Is_Used()
    {
        var result = Parse(x => x.Nickname == "n");

        result.Sql.ShouldBe("\"nick_name\" = @p0");
    }

    [Fact]
    public void Convention_Mapping_Falls_Back_To_Property_Name()
    {
        var registry = new EntityMapRegistry([]); // no explicit maps
        var parser = new ExpressionToSqlParser(registry, new TestDialect());

        var result = parser.Parse(new ExpressionSpecification<ConventionEntity>(x => x.Title == "t"));

        result.Sql.ShouldBe("\"Title\" = @p0");
        result.Parameters["p0"].ShouldBe("t");
    }

    [Fact]
    public void SqlSpecification_Is_Used_Directly_Without_Translation()
    {
        var spec = new RawSqlSpec();

        var result = _parser.Parse(spec);

        result.Sql.ShouldBe("is_active = true AND age > @minAge");
        result.Parameters["minAge"].ShouldBe(21);
    }

    [Fact]
    public void Unsupported_Expression_Throws_NotSupported()
    {
        Should.Throw<NotSupportedException>(() => Parse(x => x.Age + 1 == 2));
    }

    private sealed class RawSqlSpec : SqlSpecification<ParserEntity>
    {
        public override Expression<Func<ParserEntity, bool>> ToExpression() => x => x.Age > 21;

        public override SqlWhere ToSql() => new()
        {
            Sql = "is_active = true AND age > @minAge",
            Parameters = new Dictionary<string, object?> { ["minAge"] = 21 },
        };
    }
}

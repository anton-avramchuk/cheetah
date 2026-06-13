using System.Linq.Expressions;
using System.Text;
using Cheetah.Core.Dapper.Dialect;
using Cheetah.Core.Dapper.Mapping;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Specification;

namespace Cheetah.Core.Dapper.Specifications;

/// <summary>
/// Translates a <see cref="ISpecification{T}"/> into a <see cref="SqlWhere"/> fragment so that
/// existing <see cref="Specification{T}"/> classes can be reused on the Dapper read/write paths.
/// </summary>
/// <remarks>
/// Supported nodes: <c>&amp;&amp; || !</c>, the six comparison operators, member access on the
/// entity parameter, captured constants, and <c>string.Contains/StartsWith/EndsWith</c> (=&gt; <c>LIKE</c>).
/// <c>null</c> comparisons become <c>IS [NOT] NULL</c>. Anything else throws
/// <see cref="NotSupportedException"/> — write a <see cref="SqlSpecification{T}"/> in that case.
/// </remarks>
[Export(LifetimeType.Singleton, typeof(ISpecificationParser<SqlWhere>))]
public class ExpressionToSqlParser : ISpecificationParser<SqlWhere>
{
    private readonly IEntityMapRegistry _registry;
    private readonly ISqlDialect _dialect;

    /// <summary>Initializes a new instance of the <see cref="ExpressionToSqlParser"/> class.</summary>
    public ExpressionToSqlParser(IEntityMapRegistry registry, ISqlDialect dialect)
    {
        _registry = registry;
        _dialect = dialect;
    }

    /// <inheritdoc />
    public SqlWhere Parse<T>(ISpecification<T> specification)
    {
        if (specification is ISqlSpecification sqlSpec)
            return sqlSpec.ToSql();

        var mapping = _registry.GetMapping(typeof(T));
        var context = new TranslationContext(mapping, _dialect);
        var sql = Visit(specification.ToExpression().Body, context);

        return new SqlWhere { Sql = sql, Parameters = context.Parameters };
    }

    private static string Visit(Expression expression, TranslationContext ctx) => expression switch
    {
        BinaryExpression binary => VisitBinary(binary, ctx),
        UnaryExpression { NodeType: ExpressionType.Not } not => $"NOT ({Visit(not.Operand, ctx)})",
        UnaryExpression { NodeType: ExpressionType.Convert } convert => Visit(convert.Operand, ctx),
        MethodCallExpression call => VisitMethodCall(call, ctx),
        MemberExpression member when IsEntityMember(member) => $"{ctx.Column(member)} = {ctx.AddParameter(true)}",
        _ => throw Unsupported(expression),
    };

    private static string VisitBinary(BinaryExpression binary, TranslationContext ctx)
    {
        switch (binary.NodeType)
        {
            case ExpressionType.AndAlso:
                return $"({Visit(binary.Left, ctx)} AND {Visit(binary.Right, ctx)})";
            case ExpressionType.OrElse:
                return $"({Visit(binary.Left, ctx)} OR {Visit(binary.Right, ctx)})";
        }

        var (member, value, valueIsLeft) = SplitComparison(binary);
        var column = ctx.Column(member);

        if (value is ConstantExpression { Value: null } || EvaluatesToNull(value, out _))
        {
            return binary.NodeType switch
            {
                ExpressionType.Equal => $"{column} IS NULL",
                ExpressionType.NotEqual => $"{column} IS NOT NULL",
                _ => throw Unsupported(binary),
            };
        }

        var op = binary.NodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => valueIsLeft ? "<" : ">",
            ExpressionType.GreaterThanOrEqual => valueIsLeft ? "<=" : ">=",
            ExpressionType.LessThan => valueIsLeft ? ">" : "<",
            ExpressionType.LessThanOrEqual => valueIsLeft ? ">=" : "<=",
            _ => throw Unsupported(binary),
        };

        return $"{column} {op} {ctx.AddParameter(Evaluate(value))}";
    }

    private static string VisitMethodCall(MethodCallExpression call, TranslationContext ctx)
    {
        if (call.Object is MemberExpression target && IsEntityMember(target) && call.Arguments.Count == 1)
        {
            var column = ctx.Column(target);
            var raw = Evaluate(call.Arguments[0])?.ToString() ?? string.Empty;

            var pattern = call.Method.Name switch
            {
                nameof(string.Contains) => $"%{raw}%",
                nameof(string.StartsWith) => $"{raw}%",
                nameof(string.EndsWith) => $"%{raw}",
                _ => throw Unsupported(call),
            };

            return $"{column} LIKE {ctx.AddParameter(pattern)}";
        }

        throw Unsupported(call);
    }

    private static (MemberExpression member, Expression value, bool valueIsLeft) SplitComparison(BinaryExpression binary)
    {
        if (Unwrap(binary.Left) is MemberExpression left && IsEntityMember(left))
            return (left, binary.Right, false);
        if (Unwrap(binary.Right) is MemberExpression right && IsEntityMember(right))
            return (right, binary.Left, true);

        throw Unsupported(binary);
    }

    private static Expression Unwrap(Expression expression)
        => expression is UnaryExpression { NodeType: ExpressionType.Convert } convert ? convert.Operand : expression;

    private static bool IsEntityMember(MemberExpression member)
        => member.Expression is ParameterExpression
           || (member.Expression is MemberExpression inner && inner.Expression is ParameterExpression);

    private static bool EvaluatesToNull(Expression expression, out object? value)
    {
        value = Evaluate(expression);
        return value is null;
    }

    private static object? Evaluate(Expression expression)
    {
        if (expression is ConstantExpression constant)
            return constant.Value;

        var converted = Expression.Convert(expression, typeof(object));
        return Expression.Lambda<Func<object?>>(converted).Compile().Invoke();
    }

    private static NotSupportedException Unsupported(Expression expression)
        => new($"Expression '{expression}' cannot be translated to SQL. " +
               $"Use a {nameof(SqlSpecification<object>)} for this filter.");

    private sealed class TranslationContext
    {
        private readonly EntityMapping _mapping;
        private readonly ISqlDialect _dialect;
        private readonly Dictionary<string, object?> _parameters = new();
        private int _index;

        public TranslationContext(EntityMapping mapping, ISqlDialect dialect)
        {
            _mapping = mapping;
            _dialect = dialect;
        }

        public IReadOnlyDictionary<string, object?> Parameters => _parameters;

        public string Column(MemberExpression member)
        {
            var column = _mapping.Columns.FirstOrDefault(c => c.PropertyName == member.Member.Name)
                ?? throw new NotSupportedException(
                    $"Property '{member.Member.Name}' is not mapped for '{_mapping.EntityType.Name}'.");

            return _dialect.QuoteIdentifier(column.ColumnName);
        }

        public string AddParameter(object? value)
        {
            var name = $"p{_index++}";
            _parameters[name] = value;
            return $"{_dialect.ParameterPrefix}{name}";
        }
    }
}

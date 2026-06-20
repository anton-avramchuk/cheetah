using Cheetah.Core.DependencyInjection;
using Cheetah.Expressions;

namespace Cheetah.FeatureManagement.Filters;

/// <summary>Имена встроенных фильтров (контракт с <c>TargetingRule.FilterName</c>).</summary>
public static class BuiltInFilterNames
{
    public const string Percentage = "Percentage";
    public const string Users = "Users";
    public const string Tenants = "Tenants";
    public const string Roles = "Roles";
    public const string TimeWindow = "TimeWindow";
    public const string JsonLogic = "JsonLogic";
}

/// <summary>
/// Процентная раскатка, стабильная по <c>hash(featureKey + subject) % 100 &lt; percentage</c>.
/// Параметр: <c>percentage</c> (0–100). Субъект: UserId → TenantId → пусто.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IFeatureFilter))]
public sealed class PercentageFeatureFilter : IFeatureFilter
{
    public string Name => BuiltInFilterNames.Percentage;

    public ValueTask<bool> EvaluateAsync(FeatureFilterContext context, CancellationToken ct)
    {
        var percentage = context.Parameters.GetInt("percentage") ?? 0;
        if (percentage <= 0) return ValueTask.FromResult(false);
        if (percentage >= 100) return ValueTask.FromResult(true);

        var subject = context.Subject.UserId?.ToString()
                      ?? context.Subject.TenantId?.ToString()
                      ?? string.Empty;
        var bucket = StableHash.Bucket($"{context.FeatureKey}:{subject}");
        return ValueTask.FromResult(bucket < percentage);
    }
}

/// <summary>Allow/deny по UserId. Параметр: <c>users</c> (список Guid).</summary>
[Export(LifetimeType.Singleton, typeof(IFeatureFilter))]
public sealed class UsersFeatureFilter : IFeatureFilter
{
    public string Name => BuiltInFilterNames.Users;

    public ValueTask<bool> EvaluateAsync(FeatureFilterContext context, CancellationToken ct)
    {
        if (context.Subject.UserId is not { } userId) return ValueTask.FromResult(false);
        var users = context.Parameters.GetStringSet("users");
        return ValueTask.FromResult(users.Contains(userId.ToString(), StringComparer.OrdinalIgnoreCase));
    }
}

/// <summary>Allow/deny по TenantId. Параметр: <c>tenants</c> (список Guid).</summary>
[Export(LifetimeType.Singleton, typeof(IFeatureFilter))]
public sealed class TenantsFeatureFilter : IFeatureFilter
{
    public string Name => BuiltInFilterNames.Tenants;

    public ValueTask<bool> EvaluateAsync(FeatureFilterContext context, CancellationToken ct)
    {
        if (context.Subject.TenantId is not { } tenantId) return ValueTask.FromResult(false);
        var tenants = context.Parameters.GetStringSet("tenants");
        return ValueTask.FromResult(tenants.Contains(tenantId.ToString(), StringComparer.OrdinalIgnoreCase));
    }
}

/// <summary>Срабатывает, если у субъекта есть хотя бы одна из ролей. Параметр: <c>roles</c>.</summary>
[Export(LifetimeType.Singleton, typeof(IFeatureFilter))]
public sealed class RolesFeatureFilter : IFeatureFilter
{
    public string Name => BuiltInFilterNames.Roles;

    public ValueTask<bool> EvaluateAsync(FeatureFilterContext context, CancellationToken ct)
    {
        var required = context.Parameters.GetStringSet("roles");
        if (required.Count == 0) return ValueTask.FromResult(false);
        var has = context.Subject.Roles.Any(r => required.Contains(r, StringComparer.OrdinalIgnoreCase));
        return ValueTask.FromResult(has);
    }
}

/// <summary>Временное окно. Параметры: <c>from</c>/<c>to</c> (ISO-8601, любой опционален).</summary>
[Export(LifetimeType.Singleton, typeof(IFeatureFilter))]
public sealed class TimeWindowFeatureFilter : IFeatureFilter
{
    public string Name => BuiltInFilterNames.TimeWindow;

    public ValueTask<bool> EvaluateAsync(FeatureFilterContext context, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var from = context.Parameters.GetDate("from");
        var to = context.Parameters.GetDate("to");
        var ok = (from is null || now >= from) && (to is null || now <= to);
        return ValueTask.FromResult(ok);
    }
}

/// <summary>
/// Условие через JsonLogic над атрибутами субъекта. Параметр: <c>expression</c> (JsonLogic-строка).
/// Переиспользует тот же движок выражений, что Custom Fields/Workflow.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IFeatureFilter))]
public sealed class JsonLogicFeatureFilter : IFeatureFilter
{
    private readonly IExpressionEvaluator _evaluator;

    public JsonLogicFeatureFilter(IExpressionEvaluator evaluator) => _evaluator = evaluator;

    public string Name => BuiltInFilterNames.JsonLogic;

    public async ValueTask<bool> EvaluateAsync(FeatureFilterContext context, CancellationToken ct)
    {
        var expression = context.Parameters.GetString("expression");
        if (string.IsNullOrWhiteSpace(expression)) return false;

        // Контекст вычисления: атрибуты субъекта + стандартные поля.
        var vars = new Dictionary<string, object?>(context.Subject.Attributes, StringComparer.Ordinal)
        {
            ["userId"] = context.Subject.UserId?.ToString(),
            ["tenantId"] = context.Subject.TenantId?.ToString(),
            ["roles"] = context.Subject.Roles
        };

        return await _evaluator.EvaluateBooleanAsync(expression, vars, ct);
    }
}

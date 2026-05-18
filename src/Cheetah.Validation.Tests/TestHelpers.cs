using Cheetah.Expressions;
using Cheetah.Expressions.JsonLogic;
using Cheetah.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cheetah.Validation.Tests;

internal static class TestHelpers
{
    public static ValidationContext MakeContext(
        string fieldPath,
        object? value,
        IReadOnlyDictionary<string, object?>? allValues = null,
        IServiceProvider? services = null)
        => new(fieldPath, value, allValues ?? new Dictionary<string, object?>(), services ?? EmptyServices);

    public static readonly IServiceProvider EmptyServices = new ServiceCollection().BuildServiceProvider();

    public static IServiceProvider WithJsonLogicEvaluator()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IExpressionEvaluator>(new JsonLogicExpressionEvaluator(
            Options.Create(new ExpressionOptions()),
            NullLogger<JsonLogicExpressionEvaluator>.Instance));
        return services.BuildServiceProvider();
    }
}

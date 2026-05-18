using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Expressions.JsonLogic;

/// <summary>
/// Подключает JsonLogic-реализацию IExpressionEvaluator.
/// Биндит ExpressionOptions из секции "Expressions" в IConfiguration.
/// Регистрирует evaluator как Singleton (stateless + thread-safe).
/// </summary>
[DependsOn(typeof(CoreModule))]
public partial class CrmExpressionsJsonLogicModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();
        services.Configure<ExpressionOptions>(configuration.GetSection("Expressions"));

        RegisterServices(services);

        services.TryAddSingleton<IExpressionEvaluator, JsonLogicExpressionEvaluator>();
    }
}

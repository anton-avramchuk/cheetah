using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Validation;

/// <summary>
/// Подключает фреймворк валидации: регистрирует IValidationEngine и IValidationRuleSerializer.
/// </summary>
[DependsOn(typeof(CoreModule))]
public partial class CrmValidationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services);

        services.TryAddSingleton<IValidationEngine, ValidationEngine>();
        services.TryAddSingleton<IValidationRuleSerializer, ValidationRuleSerializer>();
    }
}

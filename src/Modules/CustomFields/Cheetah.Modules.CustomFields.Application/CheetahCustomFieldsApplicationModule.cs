using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Expressions.JsonLogic;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Domain;
using Cheetah.Modules.CustomFields.DomainEvents;
using Cheetah.Validation;

namespace Cheetah.Modules.CustomFields.Application;

/// <summary>
/// Прикладной слой CustomFields: CQRS определений/значений/реестра, валидация значений через
/// <see cref="IValidationEngine"/> и видимость полей через JsonLogic. Интеграционные события
/// публикуются через шину после <c>SaveChangesAsync</c>.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmValidationModule),
    typeof(CrmExpressionsJsonLogicModule),
    typeof(CheetahCustomFieldsDomainModule),
    typeof(CheetahCustomFieldsContractsModule),
    typeof(CheetahCustomFieldsDomainEventsModule))]
public partial class CheetahCustomFieldsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}

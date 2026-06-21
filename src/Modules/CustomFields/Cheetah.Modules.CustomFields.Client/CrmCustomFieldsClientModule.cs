using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Modules.CustomFields.Contracts;

namespace Cheetah.Modules.CustomFields.Client;

/// <summary>
/// Клиентский модуль CustomFields (server-to-server). Сам клиент и реестр типов регистрируются
/// потребителем через <c>services.AddCustomFieldsClient(...).RegisterCustomFieldTypes(...)</c>.
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CheetahCustomFieldsContractsModule))]
public partial class CrmCustomFieldsClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}

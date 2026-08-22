using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Domain;
using Cheetah.Modules.Notes.DomainEvents;

namespace Cheetah.Modules.Notes.Application;

/// <summary>
/// Прикладной слой шаблонного модуля Notes: generic CQRS заметок. Все handler'ы открыты по типу
/// заметки/DTO, поэтому закрытые регистрации делает наследник через
/// <c>AddNotesApplication&lt;…&gt;()</c> — source-генератор <c>[Export]</c> открытые generic-типы
/// не поддерживает.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CheetahNotesDomainModule),
    typeof(CheetahNotesContractsModule),
    typeof(CheetahNotesDomainEventsModule))]
public partial class CheetahNotesApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}

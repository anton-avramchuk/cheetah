using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Modules.Notes.DomainEvents;
using Cheetah.Modules.Notes.Shared;

namespace Cheetah.Modules.Notes.Domain;

/// <summary>
/// Доменный слой шаблонного модуля Notes: абстрактный агрегат <see cref="Entities.NoteBase"/> и
/// generic-спецификации фильтрации. Конкретные sealed-типы и миграции — у наследника.
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CrmDomainModule), typeof(CrmSpecificationModule),
    typeof(CrmDataAccessModule), typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CheetahNotesSharedModule), typeof(CheetahNotesDomainEventsModule))]
public partial class CheetahNotesDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}

using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Modularity;
using Cheetah.Modules.NotesTimeline.Domain;

namespace Cheetah.Modules.NotesTimeline.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля NotesTimeline: абстрактные базы EF
/// (<see cref="Persistence.NotesTimelineDbContextBase{TContext,TNote}"/>,
/// <see cref="Persistence.Configurations.NoteConfigurationBase{TNote}"/>), конфигурация ленты и
/// generic-регистрация через <c>AddNotesTimelineInfrastructure&lt;TContext,TNote&gt;()</c>. Конкретный
/// DbContext, конфигурацию заметки и миграции создаёт наследник.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CheetahNotesTimelineDomainModule))]
public partial class CheetahNotesTimelineInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}

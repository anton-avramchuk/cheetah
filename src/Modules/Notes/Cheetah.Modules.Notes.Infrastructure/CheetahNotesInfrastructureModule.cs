using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Notes.Domain;

namespace Cheetah.Modules.Notes.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля Notes: абстрактные базы EF
/// (<see cref="Persistence.NotesDbContextBase{TContext,TNote}"/>,
/// <see cref="Persistence.Configurations.NoteConfigurationBase{TNote}"/>) и generic-регистрация через
/// <c>AddNotesInfrastructure&lt;TContext,TNote&gt;()</c>. Конкретный DbContext, конфигурацию заметки и
/// миграции создаёт наследник (готовый вариант — сборка <c>Cheetah.Modules.Notes.Default</c>).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CheetahNotesDomainModule))]
public partial class CheetahNotesInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}

using Cheetah.AspNetCore;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Notes.Api;
using Cheetah.Modules.Notes.Application;
using Cheetah.Modules.Notes.Application.Extensions;
using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Default.Contracts;
using Cheetah.Modules.Notes.Default.Entities;
using Cheetah.Modules.Notes.Default.Factories;
using Cheetah.Modules.Notes.Default.Persistence;
using Cheetah.Modules.Notes.Domain;
using Cheetah.Modules.Notes.Infrastructure;
using Cheetah.Modules.Notes.Infrastructure.Extensions;

namespace Cheetah.Modules.Notes.Default;

/// <summary>
/// Модуль «из коробки»: закрывает шаблон Notes конкретными типами. В одной сборке должен быть ровно
/// один модуль (требование генератора), поэтому инфраструктура и прикладной слой регистрируются
/// здесь: <c>ConfigureServices</c> поднимает DbContext/репозиторий (<c>AddNotesInfrastructure</c>) и
/// фабрику/проектор/CQRS-handler'ы (<c>AddNotesApplication</c>). Маршруты регистрирует генератор
/// эндпоинтов — <c>OnApplicationInitialization</c> здесь намеренно не объявлен.
/// </summary>
[DependsOn(typeof(CheetahNotesDomainModule),
    typeof(CheetahNotesInfrastructureModule),
    typeof(CheetahNotesApplicationModule),
    typeof(CheetahNotesContractsModule),
    typeof(CheetahNotesApiModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmMapsterModule))]
public partial class CheetahNotesDefaultModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        services.AddNotesInfrastructure<NotesDbContext, Note>();

        services.AddNotesApplication<
            Note, CreateNoteRequest, UpdateNoteRequest, NoteDto, NoteFactory, NoteProjector>();

        RegisterServices(services);
    }
}

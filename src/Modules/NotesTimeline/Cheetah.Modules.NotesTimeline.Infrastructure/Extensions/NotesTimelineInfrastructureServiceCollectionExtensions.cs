using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.NotesTimeline.Domain.Abstractions;
using Cheetah.Modules.NotesTimeline.Domain.Entities;
using Cheetah.Modules.NotesTimeline.Infrastructure.Persistence;
using Cheetah.Modules.NotesTimeline.Infrastructure.Timeline;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.NotesTimeline.Infrastructure.Extensions;

public static class NotesTimelineInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует инфраструктуру конкретной реализации NotesTimeline: DbContext (заметки + строки
    /// ленты), мигратор, провайдер PostgreSQL, EF-репозитории <see cref="IRepository{TNote,Guid}"/> и
    /// <see cref="IRepository{TimelineEntry,Guid}"/>, а также <see cref="ITimelineWriter"/>.
    /// Вызывается из инфраструктурного модуля наследника.
    /// </summary>
    public static IServiceCollection AddNotesTimelineInfrastructure<TContext, TNote>(
        this IServiceCollection services)
        where TContext : NotesTimelineDbContextBase<TContext, TNote>
        where TNote : NoteBase
    {
        services.AddApplicationDbContext<TContext>();
        services.AddScoped<TContext>();
        services.AddDatabaseMigrator<TContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TContext>(); });
        services.AddScoped<IRepository<TNote, Guid>, EfRepository<TContext, TNote, Guid>>();
        services.AddScoped<IRepository<TimelineEntry, Guid>, EfRepository<TContext, TimelineEntry, Guid>>();
        services.AddScoped<ITimelineWriter, TimelineWriter>();
        return services;
    }
}

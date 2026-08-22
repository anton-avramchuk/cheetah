using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.Notes.Domain.Abstractions;
using Cheetah.Modules.Notes.Domain.Entities;
using Cheetah.Modules.Notes.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Notes.Infrastructure.Extensions;

public static class NotesInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует инфраструктуру конкретной реализации Notes: DbContext, мигратор, провайдер
    /// PostgreSQL и EF-репозиторий <see cref="IRepository{TNote,Guid}"/>. Вызывается из
    /// инфраструктурного модуля наследника (открытые generic-типы недоступны source-генератору
    /// <c>[Export]</c>, поэтому регистрация ручная).
    /// </summary>
    public static IServiceCollection AddNotesInfrastructure<TContext, TNote>(
        this IServiceCollection services)
        where TContext : NotesDbContextBase<TContext, TNote>
        where TNote : NoteBase
    {
        services.AddApplicationDbContext<TContext>();
        services.AddScoped<TContext>();
        services.AddDatabaseMigrator<TContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TContext>(); });
        services.AddScoped<IRepository<TNote, Guid>, EfRepository<TContext, TNote, Guid>>();
        services.AddScoped<INoteReader<TNote>, EfNoteReader<TContext, TNote>>();
        return services;
    }
}

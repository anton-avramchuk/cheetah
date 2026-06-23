using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.Teams.Domain.Entities;
using Cheetah.Modules.Teams.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Teams.Infrastructure.Extensions;

public static class TeamsInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddTeamsInfrastructure<TDbContext, TTeam, TTeamRole, TTeamMember, TTeamMemberInTeamsValue>(
        this IServiceCollection services)
        where TDbContext : TeamsDbContext<TDbContext, TTeam, TTeamRole, TTeamMember, TTeamMemberInTeamsValue>
        where TTeam : TeamBase
        where TTeamRole : TeamRoleBase
        where TTeamMember : TeamMember
        where TTeamMemberInTeamsValue : TeamMemberInTeamsValue<TTeam, TTeamMember, TTeamRole>
    {
        services.AddApplicationDbContext<TDbContext>();
        services.AddScoped<TDbContext>();
        services.AddDatabaseMigrator<TDbContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TDbContext>(); });
        services.AddScoped<IRepository<TTeam, Guid>, EfRepository<TDbContext, TTeam, Guid>>();
        services.AddScoped<IRepository<TTeamRole, Guid>, EfRepository<TDbContext, TTeamRole, Guid>>();
        services.AddScoped<IRepository<TTeamMember, Guid>, EfRepository<TDbContext, TTeamMember, Guid>>();
        return services;
    }
}
using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Crm.Recruitment.DataAccess.Configurations;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.DataAccess;

[ConnectionStringName("Recruitment")]
public class RecruitmentDbContext(DbContextOptions<RecruitmentDbContext> options)
    : CrmDbContext<RecruitmentDbContext>(options)
{
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();

    public DbSet<VacancyState> VacancyStates => Set<VacancyState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new VacancyConfiguration());
        modelBuilder.ApplyConfiguration(new VacancyStateConfiguration());
    }
}
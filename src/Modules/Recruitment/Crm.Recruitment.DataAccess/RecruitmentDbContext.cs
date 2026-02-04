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
    public DbSet<Vacancy> SampleEntities => Set<Vacancy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new VacancyConfiguration());
    }
}
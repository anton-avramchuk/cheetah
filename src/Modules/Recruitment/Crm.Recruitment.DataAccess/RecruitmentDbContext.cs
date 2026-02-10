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

    public DbSet<User> Users => Set<User>();

    public DbSet<VacancyRole> VacancyRoles => Set<VacancyRole>();

    public DbSet<VacancyAssignment> VacancyAssignments => Set<VacancyAssignment>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<CustomerDirection> CustomerDirections => Set<CustomerDirection>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<StackItem> StackItems => Set<StackItem>();

    public DbSet<WorkFormat> WorkFormats => Set<WorkFormat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new VacancyConfiguration());
        modelBuilder.ApplyConfiguration(new VacancyStateConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new VacancyRoleConfiguration());
        modelBuilder.ApplyConfiguration(new VacancyAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerDirectionConfiguration());
        modelBuilder.ApplyConfiguration(new PositionConfiguration());
        modelBuilder.ApplyConfiguration(new StackItemConfiguration());
        modelBuilder.ApplyConfiguration(new WorkFormatConfiguration());
    }
}
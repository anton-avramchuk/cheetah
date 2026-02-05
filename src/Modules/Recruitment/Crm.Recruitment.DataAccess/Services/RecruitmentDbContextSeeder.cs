using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Seeding;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.DataAccess.Services;

[Export(LifetimeType.Scoped, typeof(IDatabaseSeeder))]
public class RecruitmentDbContextSeeder(RecruitmentDbContext context) : IDatabaseSeeder
{
    private readonly RecruitmentDbContext _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!await _context.VacancyStates.AnyAsync(cancellationToken))
        {
            await _context.AddRangeAsync(
                VacancyState.Create("To do", 0),
                VacancyState.Create("In Progress", 1),
                VacancyState.Create("Pause", 2),
                VacancyState.Create("Completed", 3),
                VacancyState.Create("Cancelled", 4)
            );

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
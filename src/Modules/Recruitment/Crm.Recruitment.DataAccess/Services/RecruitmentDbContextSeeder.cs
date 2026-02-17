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
        await SeedVacancyStatesAsync(cancellationToken);
        await SeedVacancyRolesAsync(cancellationToken);
        await SeedCustomersAsync(cancellationToken);
    }

    private async Task SeedVacancyStatesAsync(CancellationToken cancellationToken)
    {
        if (await _context.VacancyStates.AnyAsync(cancellationToken))
            return;

        await _context.AddRangeAsync(
            VacancyState.Create("To do", 0, "#6c757d", isDefault: true),
            VacancyState.Create("In Progress", 1, "#0d6efd"),
            VacancyState.Create("Pause", 2, "#ffc107"),
            VacancyState.Create("Completed", 3, "#198754"),
            VacancyState.Create("Cancelled", 4, "#dc3545")
        );

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCustomersAsync(CancellationToken cancellationToken)
    {
        if (await _context.Customers.AnyAsync(cancellationToken))
            return;

        await _context.AddRangeAsync(
            Customer.Create("Sberbank", code: "SBER"),
            Customer.Create("VTB Bank", code: "VTB"),
            Customer.Create("Yandex", code: "YND")
        );

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedVacancyRolesAsync(CancellationToken cancellationToken)
    {
        if (await _context.VacancyRoles.AnyAsync(cancellationToken))
            return;

        await _context.AddRangeAsync(
            VacancyRole.Create("Lead", "lead", isSingle: true, order: 0),
            VacancyRole.Create("Recruiter", "recruiter", isSingle: false, order: 1),
            VacancyRole.Create("Sourcer", "sourcer", isSingle: false, order: 2)
        );

        await _context.SaveChangesAsync(cancellationToken);
    }
}
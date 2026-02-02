using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Recruitment.DataAccess;

public class RecruitmentDbContextFactory : IDesignTimeDbContextFactory<RecruitmentDbContext>
{
    public RecruitmentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RecruitmentDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=recruitment;Username=postgres;Password=postgres");

        return new RecruitmentDbContext(optionsBuilder.Options);
    }
}
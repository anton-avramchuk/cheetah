using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Candidates.DataAccess;

public class CandidatesDbContextFactory : IDesignTimeDbContextFactory<CandidatesDbContext>
{
    public CandidatesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CandidatesDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=candidates;Username=postgres;Password=postgres");

        return new CandidatesDbContext(optionsBuilder.Options);
    }
}
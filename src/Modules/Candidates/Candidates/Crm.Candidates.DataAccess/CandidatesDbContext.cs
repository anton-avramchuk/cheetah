using Cheetah.Core.EntityFramework;
using Crm.Candidates.DataAccess.Configurations;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.DataAccess;

public class CandidatesDbContext(DbContextOptions<CandidatesDbContext> options)
    : CrmDbContext<CandidatesDbContext>(options)
{
    public DbSet<Candidate> SampleEntities => Set<Candidate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CandidateConfiguration());
    }
}
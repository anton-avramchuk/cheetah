using Cheetah.Core.EntityFramework;
using Crm.Candidates.DataAccess.Configurations;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Candidates.DataAccess;

public class CandidatesDbContext(DbContextOptions<CandidatesDbContext> options)
    : CrmDbContext<CandidatesDbContext>(options)
{
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<CandidateApplication> CandidateApplications => Set<CandidateApplication>();
    public DbSet<CandidateStage> CandidateStages => Set<CandidateStage>();
    public DbSet<CandidateSource> CandidateSources => Set<CandidateSource>();
    public DbSet<CandidateExternalProfile> CandidateExternalProfiles => Set<CandidateExternalProfile>();
    public DbSet<CandidateComment> CandidateComments => Set<CandidateComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CandidateConfiguration());
        modelBuilder.ApplyConfiguration(new CandidateApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new CandidateStageConfiguration());
        modelBuilder.ApplyConfiguration(new CandidateSourceConfiguration());
        modelBuilder.ApplyConfiguration(new CandidateExternalProfileConfiguration());
        modelBuilder.ApplyConfiguration(new CandidateCommentConfiguration());
    }
}

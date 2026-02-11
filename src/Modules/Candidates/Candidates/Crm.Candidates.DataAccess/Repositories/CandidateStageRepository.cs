using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Crm.Candidates.Domain;

namespace Crm.Candidates.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<CandidateStage, Guid>))]
public class CandidateStageRepository : EfRepository<CandidatesDbContext, CandidateStage>
{
    public CandidateStageRepository(CandidatesDbContext context) : base(context)
    {
    }
}

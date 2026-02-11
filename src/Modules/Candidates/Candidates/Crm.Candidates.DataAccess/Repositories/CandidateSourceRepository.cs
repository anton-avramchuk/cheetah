using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Crm.Candidates.Domain;

namespace Crm.Candidates.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<CandidateSource, Guid>))]
public class CandidateSourceRepository : EfRepository<CandidatesDbContext, CandidateSource>
{
    public CandidateSourceRepository(CandidatesDbContext context) : base(context)
    {
    }
}

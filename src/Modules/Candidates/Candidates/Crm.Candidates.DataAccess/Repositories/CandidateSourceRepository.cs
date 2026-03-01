using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Candidates.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Candidates.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CandidateSource>), typeof(IRepository<CandidateSource, Guid>))]
public class CandidateSourceRepository : EfGridRepository<CandidatesDbContext, CandidateSource>
{
    public CandidateSourceRepository(CandidatesDbContext context, IObjectMapper mapper, ILogger<CandidateSourceRepository> logger)
        : base(context, mapper, logger)
    {
    }
}

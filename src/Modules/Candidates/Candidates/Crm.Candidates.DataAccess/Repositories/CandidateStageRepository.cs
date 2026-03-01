using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Candidates.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Candidates.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CandidateStage>), typeof(IRepository<CandidateStage, Guid>))]
public class CandidateStageRepository : EfGridRepository<CandidatesDbContext, CandidateStage>
{
    public CandidateStageRepository(CandidatesDbContext context, IObjectMapper mapper, ILogger<CandidateStageRepository> logger)
        : base(context, mapper, logger)
    {
    }
}

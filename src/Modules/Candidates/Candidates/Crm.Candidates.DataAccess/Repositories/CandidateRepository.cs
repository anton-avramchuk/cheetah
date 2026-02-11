using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Candidates.Domain;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.Logging;

namespace Crm.Candidates.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Candidate>), typeof(IRepository<Candidate, Guid>))]
public class CandidateRepository : EfGridRepository<CandidatesDbContext, Candidate>
{
    public CandidateRepository(CandidatesDbContext context, IObjectMapper mapper, ILogger<CandidateRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
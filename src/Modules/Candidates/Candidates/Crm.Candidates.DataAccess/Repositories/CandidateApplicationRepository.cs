using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Candidates.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Candidates.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CandidateApplication>), typeof(IRepository<CandidateApplication, Guid>))]
public class CandidateApplicationRepository : EfGridRepository<CandidatesDbContext, CandidateApplication>
{
    public CandidateApplicationRepository(CandidatesDbContext context, IObjectMapper mapper, ILogger<CandidateApplicationRepository> logger)
        : base(context, mapper, logger)
    {
    }
}

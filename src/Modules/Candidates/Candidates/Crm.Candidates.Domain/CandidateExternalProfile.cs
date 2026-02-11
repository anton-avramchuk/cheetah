using Cheetah.Core.Domain;

namespace Crm.Candidates.Domain;

public class CandidateExternalProfile : Entity<Guid>
{
    public Guid CandidateId { get; private set; }
    public Guid SourceId { get; private set; }
    public CandidateSource Source { get; private set; } = null!;
    public string? Url { get; private set; }
    public string? ExternalId { get; private set; }

    private CandidateExternalProfile()
    {
    }

    internal static CandidateExternalProfile Create(Guid candidateId, Guid sourceId, string? url = null, string? externalId = null)
    {
        return new CandidateExternalProfile
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            SourceId = sourceId,
            Url = url,
            ExternalId = externalId
        };
    }
}

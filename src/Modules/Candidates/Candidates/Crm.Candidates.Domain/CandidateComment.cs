using Cheetah.Core.Domain;

namespace Crm.Candidates.Domain;

public class CandidateComment : Entity<Guid>, ICreateAtEntity
{
    public Guid CandidateId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Text { get; private set; } = null!;
    public DateTimeOffset? CreatedAt { get; set; }

    private CandidateComment()
    {
    }

    internal static CandidateComment Create(Guid candidateId, Guid authorId, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        return new CandidateComment
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            AuthorId = authorId,
            Text = text
        };
    }
}

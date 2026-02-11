using Cheetah.Core.Domain;
using Crm.Candidates.DomainEvents;

namespace Crm.Candidates.Domain;

public class Candidate : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? City { get; private set; }
    public string? CurrentPosition { get; private set; }
    public string? CurrentCompany { get; private set; }
    public decimal? SalaryExpectation { get; private set; }
    public string? About { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private readonly List<CandidateExternalProfile> _externalProfiles = [];
    private readonly List<CandidateComment> _comments = [];
    public IReadOnlyCollection<CandidateExternalProfile> ExternalProfiles => _externalProfiles.AsReadOnly();
    public IReadOnlyCollection<CandidateComment> Comments => _comments.AsReadOnly();

    private Candidate()
    {
    }

    public static Candidate Create(
        string firstName,
        string lastName,
        string? email = null,
        string? phone = null,
        string? city = null,
        string? currentPosition = null,
        string? currentCompany = null,
        decimal? salaryExpectation = null,
        string? about = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        var entity = new Candidate
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            City = city,
            CurrentPosition = currentPosition,
            CurrentCompany = currentCompany,
            SalaryExpectation = salaryExpectation,
            About = about
        };

        entity.AddDomainEvent(new CandidateCreatedEvent(entity.Id, firstName, lastName));
        return entity;
    }

    public void Update(
        string firstName,
        string lastName,
        string? email = null,
        string? phone = null,
        string? city = null,
        string? currentPosition = null,
        string? currentCompany = null,
        decimal? salaryExpectation = null,
        string? about = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        City = city;
        CurrentPosition = currentPosition;
        CurrentCompany = currentCompany;
        SalaryExpectation = salaryExpectation;
        About = about;
    }

    public CandidateExternalProfile AddExternalProfile(Guid sourceId, string? url = null, string? externalId = null)
    {
        var profile = CandidateExternalProfile.Create(Id, sourceId, url, externalId);
        _externalProfiles.Add(profile);
        return profile;
    }

    public void RemoveExternalProfile(Guid profileId)
    {
        var profile = _externalProfiles.Find(p => p.Id == profileId);
        if (profile is not null)
            _externalProfiles.Remove(profile);
    }

    public CandidateComment AddComment(Guid authorId, string text)
    {
        var comment = CandidateComment.Create(Id, authorId, text);
        _comments.Add(comment);
        return comment;
    }
}

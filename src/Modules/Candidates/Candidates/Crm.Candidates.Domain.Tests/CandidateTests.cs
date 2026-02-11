using Crm.Candidates.Domain;
using Shouldly;

namespace Crm.Candidates.Domain.Tests;

public class CandidateTests
{
    [Fact]
    public void Create_WithValidNames_ShouldCreateEntity()
    {
        var entity = Candidate.Create("John", "Doe");

        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.FirstName.ShouldBe("John");
        entity.LastName.ShouldBe("Doe");
        entity.Email.ShouldBeNull();
        entity.Phone.ShouldBeNull();
        entity.City.ShouldBeNull();
        entity.CurrentPosition.ShouldBeNull();
        entity.CurrentCompany.ShouldBeNull();
        entity.SalaryExpectation.ShouldBeNull();
        entity.About.ShouldBeNull();
    }

    [Fact]
    public void Create_WithAllFields_ShouldCreateEntity()
    {
        var entity = Candidate.Create(
            "John", "Doe",
            email: "john@test.com",
            phone: "+1234567890",
            city: "Moscow",
            currentPosition: "Developer",
            currentCompany: "Acme",
            salaryExpectation: 150000m,
            about: "Experienced developer");

        entity.FirstName.ShouldBe("John");
        entity.LastName.ShouldBe("Doe");
        entity.Email.ShouldBe("john@test.com");
        entity.Phone.ShouldBe("+1234567890");
        entity.City.ShouldBe("Moscow");
        entity.CurrentPosition.ShouldBe("Developer");
        entity.CurrentCompany.ShouldBe("Acme");
        entity.SalaryExpectation.ShouldBe(150000m);
        entity.About.ShouldBe("Experienced developer");
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var entity = Candidate.Create("John", "Doe");

        entity.DomainEvents.Count.ShouldBe(1);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var entity1 = Candidate.Create("John", "Doe");
        var entity2 = Candidate.Create("Jane", "Smith");

        entity1.Id.ShouldNotBe(entity2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFirstName_ShouldThrowArgumentException(string? firstName)
    {
        var act = () => Candidate.Create(firstName!, "Doe");

        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidLastName_ShouldThrowArgumentException(string? lastName)
    {
        var act = () => Candidate.Create("John", lastName!);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateEntity()
    {
        var entity = Candidate.Create("John", "Doe");

        entity.Update("Jane", "Smith", email: "jane@test.com", city: "London");

        entity.FirstName.ShouldBe("Jane");
        entity.LastName.ShouldBe("Smith");
        entity.Email.ShouldBe("jane@test.com");
        entity.City.ShouldBe("London");
    }

    [Fact]
    public void Update_ShouldNotChangeId()
    {
        var entity = Candidate.Create("John", "Doe");
        var originalId = entity.Id;

        entity.Update("Jane", "Smith");

        entity.Id.ShouldBe(originalId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidFirstName_ShouldThrowArgumentException(string? firstName)
    {
        var entity = Candidate.Create("John", "Doe");

        var act = () => entity.Update(firstName!, "Smith");

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void AddExternalProfile_ShouldAddProfile()
    {
        var entity = Candidate.Create("John", "Doe");
        var sourceId = Guid.NewGuid();

        var profile = entity.AddExternalProfile(sourceId, "https://linkedin.com/in/johndoe", "ext-123");

        entity.ExternalProfiles.Count.ShouldBe(1);
        profile.CandidateId.ShouldBe(entity.Id);
        profile.SourceId.ShouldBe(sourceId);
        profile.Url.ShouldBe("https://linkedin.com/in/johndoe");
        profile.ExternalId.ShouldBe("ext-123");
    }

    [Fact]
    public void RemoveExternalProfile_ShouldRemoveProfile()
    {
        var entity = Candidate.Create("John", "Doe");
        var profile = entity.AddExternalProfile(Guid.NewGuid());

        entity.RemoveExternalProfile(profile.Id);

        entity.ExternalProfiles.Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveExternalProfile_WithNonExistingId_ShouldNotThrow()
    {
        var entity = Candidate.Create("John", "Doe");

        entity.RemoveExternalProfile(Guid.NewGuid());

        entity.ExternalProfiles.Count.ShouldBe(0);
    }

    [Fact]
    public void AddComment_ShouldAddComment()
    {
        var entity = Candidate.Create("John", "Doe");
        var authorId = Guid.NewGuid();

        var comment = entity.AddComment(authorId, "Great candidate");

        entity.Comments.Count.ShouldBe(1);
        comment.CandidateId.ShouldBe(entity.Id);
        comment.AuthorId.ShouldBe(authorId);
        comment.Text.ShouldBe("Great candidate");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddComment_WithInvalidText_ShouldThrowArgumentException(string? text)
    {
        var entity = Candidate.Create("John", "Doe");

        var act = () => entity.AddComment(Guid.NewGuid(), text!);

        Should.Throw<ArgumentException>(act);
    }
}

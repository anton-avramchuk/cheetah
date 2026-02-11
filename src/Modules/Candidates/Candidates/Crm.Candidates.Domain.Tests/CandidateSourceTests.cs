using Crm.Candidates.Domain;
using Shouldly;

namespace Crm.Candidates.Domain.Tests;

public class CandidateSourceTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateEntity()
    {
        var entity = CandidateSource.Create("LinkedIn", 1, "#0077b5");

        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.Name.ShouldBe("LinkedIn");
        entity.Order.ShouldBe(1);
        entity.Color.ShouldBe("#0077b5");
    }

    [Fact]
    public void Create_WithMinimalParams_ShouldUseDefaults()
    {
        var entity = CandidateSource.Create("hh.ru");

        entity.Order.ShouldBe(0);
        entity.Color.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var act = () => CandidateSource.Create(name!);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllFields()
    {
        var entity = CandidateSource.Create("LinkedIn", 1, "#0077b5");

        entity.Update("HeadHunter", 2, "#d6001c");

        entity.Name.ShouldBe("HeadHunter");
        entity.Order.ShouldBe(2);
        entity.Color.ShouldBe("#d6001c");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var entity = CandidateSource.Create("LinkedIn");

        var act = () => entity.Update(name!, 0);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var entity1 = CandidateSource.Create("LinkedIn");
        var entity2 = CandidateSource.Create("hh.ru");

        entity1.Id.ShouldNotBe(entity2.Id);
    }
}

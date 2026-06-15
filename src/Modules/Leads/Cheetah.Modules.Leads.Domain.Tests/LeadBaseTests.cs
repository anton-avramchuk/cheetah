using Cheetah.Modules.Leads.DomainEvents;
using Cheetah.Modules.Leads.Shared;
using Shouldly;

namespace Cheetah.Modules.Leads.Domain.Tests;

public class LeadBaseTests
{
    [Fact]
    public void Create_SetsNew_AndRaisesCreatedEvent()
    {
        var lead = TestLead.Create("John Doe", LeadSource.Web, "john@acme.io", "+12025550123", "Acme");

        lead.Id.ShouldNotBe(Guid.Empty);
        lead.Status.ShouldBe(LeadStatus.New);
        lead.FullName.ShouldBe("John Doe");
        lead.Email!.Value.ShouldBe("john@acme.io");
        lead.Phone!.Value.ShouldBe("+12025550123");

        lead.DomainEvents.OfType<LeadCreatedIntegrationEvent>().ShouldHaveSingleItem()
            .Source.ShouldBe(nameof(LeadSource.Web));
    }

    [Fact]
    public void Create_TrimsName()
        => TestLead.Create("  John  ", LeadSource.Manual).FullName.ShouldBe("John");

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankName_Throws(string name)
        => Should.Throw<ArgumentException>(() => TestLead.Create(name, LeadSource.Manual));

    [Fact]
    public void Create_InvalidEmail_Throws()
        => Should.Throw<ArgumentException>(() => TestLead.Create("John", LeadSource.Web, "not-an-email"));

    [Fact]
    public void Create_NullContacts_LeavesVoNull()
    {
        var lead = TestLead.Create("John", LeadSource.Manual);
        lead.Email.ShouldBeNull();
        lead.Phone.ShouldBeNull();
    }

    [Fact]
    public void Score_DefaultRule_SumsWeights()
    {
        // email 30 + phone 30 + company 20 + referral 20 = 100
        var lead = TestLead.Create("John", LeadSource.Referral, "j@x.io", "+12025550123", "Acme");
        lead.Score.ShouldBe(100);
    }

    [Fact]
    public void Score_Override_IsRespected()
        => CustomScoreLead.Create("John", LeadSource.Manual).Score.ShouldBe(100);

    [Fact]
    public void Qualify_FromNew_SetsQualified_AndRaisesEvent()
    {
        var lead = TestLead.Create("John", LeadSource.Web);
        lead.Qualify();
        lead.Status.ShouldBe(LeadStatus.Qualified);
        lead.DomainEvents.OfType<LeadQualifiedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Qualify_FromConverted_Throws()
    {
        var lead = TestLead.Create("John", LeadSource.Web);
        lead.Qualify();
        lead.MarkConverted(Guid.NewGuid(), null);
        Should.Throw<InvalidOperationException>(() => lead.Qualify());
    }

    [Fact]
    public void Disqualify_RequiresReason()
        => Should.Throw<ArgumentException>(() => TestLead.Create("John", LeadSource.Web).Disqualify("  "));

    [Fact]
    public void Disqualify_SetsStatus_Reason_AndEvent()
    {
        var lead = TestLead.Create("John", LeadSource.Web);
        lead.Disqualify("no budget");
        lead.Status.ShouldBe(LeadStatus.Disqualified);
        lead.DisqualifyReason.ShouldBe("no budget");
        lead.DomainEvents.OfType<LeadDisqualifiedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void MarkConverted_SetsIds_AndEvent()
    {
        var lead = TestLead.Create("John", LeadSource.Web);
        lead.Qualify();
        var customerId = Guid.NewGuid();
        var dealId = Guid.NewGuid();

        lead.MarkConverted(customerId, dealId);

        lead.Status.ShouldBe(LeadStatus.Converted);
        lead.ConvertedCustomerId.ShouldBe(customerId);
        lead.ConvertedDealId.ShouldBe(dealId);
        lead.DomainEvents.OfType<LeadConvertedIntegrationEvent>().ShouldHaveSingleItem()
            .CustomerId.ShouldBe(customerId);
    }

    [Fact]
    public void MarkConverted_Twice_IsIdempotent()
    {
        var lead = TestLead.Create("John", LeadSource.Web);
        lead.Qualify();
        lead.MarkConverted(Guid.NewGuid(), null);
        lead.MarkConverted(Guid.NewGuid(), null);
        lead.DomainEvents.OfType<LeadConvertedIntegrationEvent>().Count().ShouldBe(1);
    }

    [Fact]
    public void Update_RecalculatesScore()
    {
        var lead = TestLead.Create("John", LeadSource.Manual);
        lead.Score.ShouldBe(0);
        lead.Update("John", "Acme", "j@x.io", "+12025550123");
        lead.Score.ShouldBe(80); // email 30 + phone 30 + company 20
    }

    [Fact]
    public void ExtensionField_IsIndependentOfBase()
    {
        var lead = TestLead.Create("John", LeadSource.Web);
        lead.SetIndustry("IT");
        lead.Industry.ShouldBe("IT");
    }
}

using Cheetah.Modules.Leads.DomainEvents;
using Cheetah.Modules.Leads.Shared;
using Shouldly;

namespace Cheetah.Modules.Leads.Domain.Tests;

public class LeadBaseTests
{
    [Fact]
    public void Create_SetsNewStatus_AndRaisesCreatedEvent()
    {
        var lead = TestLead.Create("John Doe", LeadWellKnownIds.SourceWeb, "john@acme.io", "+12025550123", "Acme");

        lead.Id.ShouldNotBe(Guid.Empty);
        lead.StatusId.ShouldBe(LeadWellKnownIds.StatusNew);
        lead.SourceId.ShouldBe(LeadWellKnownIds.SourceWeb);
        lead.FullName.ShouldBe("John Doe");
        lead.Email!.Value.ShouldBe("john@acme.io");

        lead.DomainEvents.OfType<LeadCreatedIntegrationEvent>().ShouldHaveSingleItem()
            .SourceId.ShouldBe(LeadWellKnownIds.SourceWeb);
    }

    [Fact]
    public void Create_EmptySource_Throws()
        => Should.Throw<ArgumentException>(() => TestLead.Create("John", Guid.Empty));

    [Fact]
    public void Create_TrimsName()
        => TestLead.Create("  John  ").FullName.ShouldBe("John");

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankName_Throws(string name)
        => Should.Throw<ArgumentException>(() => TestLead.Create(name));

    [Fact]
    public void Create_InvalidEmail_Throws()
        => Should.Throw<ArgumentException>(() => TestLead.Create("John", email: "not-an-email"));

    [Fact]
    public void Score_DefaultRule_SumsWeights()
    {
        // email 30 + phone 30 + company 20 + referral 20 = 100
        var lead = TestLead.Create("John", LeadWellKnownIds.SourceReferral, "j@x.io", "+12025550123", "Acme");
        lead.Score.ShouldBe(100);
    }

    [Fact]
    public void Score_Override_IsRespected()
        => CustomScoreLead.Create("John").Score.ShouldBe(100);

    [Fact]
    public void Qualify_FromNew_SetsQualified_AndRaisesEvent()
    {
        var lead = TestLead.Create("John");
        lead.Qualify();
        lead.StatusId.ShouldBe(LeadWellKnownIds.StatusQualified);
        lead.DomainEvents.OfType<LeadQualifiedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Qualify_FromConverted_Throws()
    {
        var lead = TestLead.Create("John");
        lead.Qualify();
        lead.MarkConverted(Guid.NewGuid(), null);
        Should.Throw<InvalidOperationException>(() => lead.Qualify());
    }

    [Fact]
    public void Disqualify_RequiresReason()
        => Should.Throw<ArgumentException>(() => TestLead.Create("John").Disqualify("  "));

    [Fact]
    public void Disqualify_SetsStatus_Reason_AndEvent()
    {
        var lead = TestLead.Create("John");
        lead.Disqualify("no budget");
        lead.StatusId.ShouldBe(LeadWellKnownIds.StatusDisqualified);
        lead.DisqualifyReason.ShouldBe("no budget");
        lead.DomainEvents.OfType<LeadDisqualifiedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void MarkConverted_SetsIds_AndEvent()
    {
        var lead = TestLead.Create("John");
        lead.Qualify();
        var customerId = Guid.NewGuid();
        var dealId = Guid.NewGuid();

        lead.MarkConverted(customerId, dealId);

        lead.StatusId.ShouldBe(LeadWellKnownIds.StatusConverted);
        lead.ConvertedCustomerId.ShouldBe(customerId);
        lead.ConvertedDealId.ShouldBe(dealId);
        lead.DomainEvents.OfType<LeadConvertedIntegrationEvent>().ShouldHaveSingleItem()
            .CustomerId.ShouldBe(customerId);
    }

    [Fact]
    public void MarkConverted_Twice_IsIdempotent()
    {
        var lead = TestLead.Create("John");
        lead.Qualify();
        lead.MarkConverted(Guid.NewGuid(), null);
        lead.MarkConverted(Guid.NewGuid(), null);
        lead.DomainEvents.OfType<LeadConvertedIntegrationEvent>().Count().ShouldBe(1);
    }

    [Fact]
    public void Update_RecalculatesScore()
    {
        var lead = TestLead.Create("John", LeadWellKnownIds.SourceManual);
        lead.Score.ShouldBe(0);
        lead.Update("John", "Acme", "j@x.io", "+12025550123");
        lead.Score.ShouldBe(80); // email 30 + phone 30 + company 20
    }

    [Fact]
    public void ExtensionField_IsIndependentOfBase()
    {
        var lead = TestLead.Create("John");
        lead.SetIndustry("IT");
        lead.Industry.ShouldBe("IT");
    }
}

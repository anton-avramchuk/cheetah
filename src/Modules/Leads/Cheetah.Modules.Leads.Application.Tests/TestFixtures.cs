using Cheetah.Modules.Leads.Application.Abstractions;
using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Application.Tests;

/// <summary>Конкретный лид наследника с доп. полем — для проверки generic-хендлеров.</summary>
public sealed class TestLead : LeadBase
{
    public string? Industry { get; private set; }

    private TestLead() { }

    public static TestLead Create(TestCreateRequest r)
    {
        var lead = new TestLead();
        lead.InitializeCore(Guid.NewGuid(), r.FullName, r.Source, r.Email, r.Phone, r.Company, r.OwnerId);
        lead.Industry = r.Industry;
        return lead;
    }
}

public sealed record TestCreateRequest : CreateLeadRequestBase
{
    public string? Industry { get; init; }
}

public sealed record TestUpdateRequest : UpdateLeadRequestBase;

public sealed record TestConvertRequest : ConvertLeadRequestBase;

public sealed record TestLeadDto : LeadDtoBase
{
    public string? Industry { get; init; }
}

public sealed class TestLeadFactory : ILeadFactory<TestLead, TestCreateRequest>
{
    public TestLead Create(TestCreateRequest request) => TestLead.Create(request);
}

public sealed class TestLeadProjector : ILeadProjector<TestLead, TestLeadDto>
{
    public TestLeadDto ToDto(TestLead l) => new()
    {
        Id = l.Id,
        FullName = l.FullName,
        Company = l.Company,
        Email = l.Email?.Value,
        Phone = l.Phone?.Value,
        Source = l.Source,
        Status = l.Status,
        Score = l.Score,
        OwnerId = l.OwnerId,
        ConvertedCustomerId = l.ConvertedCustomerId,
        ConvertedDealId = l.ConvertedDealId,
        DisqualifyReason = l.DisqualifyReason,
        CreatedAt = l.CreatedAt,
        UpdatedAt = l.UpdatedAt,
        Industry = l.Industry
    };
}

internal static class TestData
{
    public static TestCreateRequest CreateRequest(string fullName = "John Doe", string? email = null) => new()
    {
        FullName = fullName,
        Source = LeadSource.Web,
        Email = email,
        OwnerId = Guid.NewGuid()
    };

    public static TestLead NewLead() => TestLead.Create(CreateRequest());

    public static TestLead QualifiedLead()
    {
        var lead = NewLead();
        lead.Qualify();
        return lead;
    }
}

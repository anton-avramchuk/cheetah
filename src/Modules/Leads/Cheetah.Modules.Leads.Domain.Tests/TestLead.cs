using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Domain.Tests;

/// <summary>
/// Конкретный наследник <see cref="LeadBase"/> для проверки базового поведения. Доп. поле
/// <see cref="Industry"/> демонстрирует расширяемость сущности.
/// </summary>
public sealed class TestLead : LeadBase
{
    public string? Industry { get; private set; }

    private TestLead() { }

    public static TestLead Create(string fullName, Guid? sourceId = null,
        string? email = null, string? phone = null, string? company = null, Guid? ownerId = null)
    {
        var lead = new TestLead();
        lead.InitializeCore(Guid.NewGuid(), fullName, sourceId ?? LeadWellKnownIds.SourceWeb,
            email, phone, company, ownerId);
        return lead;
    }

    public void SetIndustry(string industry) => Industry = industry;
}

/// <summary>Наследник с переопределённым скорингом — проверка расширяемости правила.</summary>
public sealed class CustomScoreLead : LeadBase
{
    private CustomScoreLead() { }

    public static CustomScoreLead Create(string fullName)
    {
        var lead = new CustomScoreLead();
        lead.InitializeCore(Guid.NewGuid(), fullName, LeadWellKnownIds.SourceManual, null, null, null, null);
        return lead;
    }

    protected override int CalculateScore() => 100;
}

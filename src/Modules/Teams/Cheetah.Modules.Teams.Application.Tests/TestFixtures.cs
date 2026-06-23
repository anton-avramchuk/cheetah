using Cheetah.Modules.Teams.Application.Abstractions;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.Tests;

/// <summary>Конкретная команда наследника с доп. полем — для проверки generic-хендлеров.</summary>
public sealed class TestTeam : TeamBase
{
    public string? Department { get; private set; }

    private TestTeam() { }

    public static TestTeam Create(TestCreateTeamRequest r)
    {
        var team = new TestTeam();
        team.InitializeCore(Guid.NewGuid(), r.Name);
        team.Department = r.Department;
        return team;
    }
}

public sealed record TestCreateTeamRequest : CreateTeamRequestBase
{
    public string? Department { get; init; }
}

public sealed record TestUpdateTeamRequest : UpdateTeamRequestBase;

public sealed record TestTeamDto : TeamDtoBase
{
    public string? Department { get; init; }
}

public sealed record TestTeamGridViewModel : TeamGridViewModelBase
{
    public string? Department { get; init; }
}

public sealed class TestTeamFactory : ITeamFactory<TestTeam, TestCreateTeamRequest>
{
    public TestTeam Create(TestCreateTeamRequest request) => TestTeam.Create(request);
}

public sealed class TestTeamProjector : ITeamProjector<TestTeam, TestTeamDto>
{
    public TestTeamDto ToDto(TestTeam t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        IsActive = t.IsActive,
        Members = t.Members
            .Select(m => new TeamMembershipDto { MemberId = m.MemberId, RoleId = m.RoleId })
            .ToArray(),
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        Department = t.Department
    };
}

internal static class TestData
{
    public static TestCreateTeamRequest CreateRequest(string name = "Alpha") => new() { Name = name };

    public static TestTeam NewTeam(string name = "Alpha") => TestTeam.Create(CreateRequest(name));
}

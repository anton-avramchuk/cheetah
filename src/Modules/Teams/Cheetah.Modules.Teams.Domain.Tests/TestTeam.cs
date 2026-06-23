using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Domain.Tests;

/// <summary>Конкретная команда наследника с доп. полем — для проверки базового агрегата.</summary>
public sealed class TestTeam : TeamBase
{
    public string? Department { get; private set; }

    private TestTeam() { }

    public static TestTeam Create(string name, string? department = null)
    {
        var team = new TestTeam();
        team.InitializeCore(Guid.NewGuid(), name);
        team.Department = department;
        return team;
    }
}

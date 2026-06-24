namespace Cheetah.Modules.Teams.Infrastructure.Identity;

/// <summary>
/// Настройки фоновой синхронизации реплики участников из Identity. Наследник может переопределить
/// их через <c>Configure&lt;TeamMemberSyncOptions&gt;</c> или конфигурацию.
/// </summary>
public sealed class TeamMemberSyncOptions
{
    public const string SectionName = "Teams:MemberSync";

    /// <summary>Включена ли фоновая синхронизация.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Запускать первый синк сразу при старте (не дожидаясь интервала).</summary>
    public bool RunOnStartup { get; set; } = true;

    /// <summary>Период полного синка. По умолчанию — раз в 5 минут.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);
}

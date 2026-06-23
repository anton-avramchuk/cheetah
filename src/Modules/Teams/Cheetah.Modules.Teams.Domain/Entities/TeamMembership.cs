using Cheetah.Core.Domain;

namespace Cheetah.Modules.Teams.Domain.Entities;

/// <summary>
/// Членство — дитя агрегата <see cref="TeamBase"/>: связывает участника (<see cref="MemberId"/>) с
/// его ролью (<see cref="RoleId"/>) в конкретной команде. Ссылается на участника и роль по
/// идентификатору (без навигаций к абстрактным/справочным типам — как
/// <c>PriceListItem.ProductId</c>). Создаётся/изменяется только через агрегат, поэтому фабрика и
/// мутатор — <c>internal</c>.
/// </summary>
public sealed class TeamMembership : Entity<Guid>
{
    public Guid TeamId { get; private set; }
    public Guid MemberId { get; private set; }
    public Guid RoleId { get; private set; }

    private TeamMembership() { } // EF

    internal static TeamMembership Create(Guid teamId, Guid memberId, Guid roleId)
    {
        if (memberId == Guid.Empty)
            throw new ArgumentException("MemberId is required.", nameof(memberId));
        if (roleId == Guid.Empty)
            throw new ArgumentException("RoleId is required.", nameof(roleId));

        return new TeamMembership
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            MemberId = memberId,
            RoleId = roleId
        };
    }

    internal void ChangeRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("RoleId is required.", nameof(roleId));
        RoleId = roleId;
    }
}

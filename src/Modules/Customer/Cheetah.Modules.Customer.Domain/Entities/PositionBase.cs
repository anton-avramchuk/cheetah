using Cheetah.Core.Domain;

namespace Cheetah.Modules.Customer.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат справочной должности контактного лица. Шаблонный модуль не
/// инстанцирует его сам: наследник объявляет конкретный <c>sealed class Position : PositionBase</c>
/// со своей фабрикой. Контакт ссылается на должность через <see cref="ContactBase{TPosition}"/>.
/// </summary>
public abstract class PositionBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected PositionBase() { } // EF

    /// <summary>Заводит инварианты новой должности. Вызывается фабрикой наследника.</summary>
    protected void InitializeCore(Guid id, string name)
    {
        Id = id;
        SetName(name);
    }

    public void Rename(string name) => SetName(name);

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }
}

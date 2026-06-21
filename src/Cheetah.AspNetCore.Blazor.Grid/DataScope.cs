namespace Cheetah.AspNetCore.Blazor.Grid;

/// <summary>Контекст видимости данных текущего пользователя для скоупинга гридов.</summary>
/// <param name="UserId">Идентификатор текущего пользователя.</param>
/// <param name="CanViewAll">true — видит все записи (есть право data.viewAll или admin); false — только свои.</param>
public sealed record DataScope(Guid UserId, bool CanViewAll);

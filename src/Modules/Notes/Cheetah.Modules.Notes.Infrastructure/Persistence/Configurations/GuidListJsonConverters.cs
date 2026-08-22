using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cheetah.Modules.Notes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конвертер/компаратор для хранения коллекции <see cref="Guid"/> в колонке <c>jsonb</c>. Используется
/// для упоминаний и вложений заметки: меньше джойнов на карточке, GIN-индексация при необходимости.
/// </summary>
internal static class GuidListJsonConverters
{
    public static readonly ValueConverter<IReadOnlyList<Guid>, string> Converter = new(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>());

    public static readonly ValueComparer<IReadOnlyList<Guid>> Comparer = new(
        (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
        v => v.Aggregate(0, (acc, id) => HashCode.Combine(acc, id.GetHashCode())),
        v => v.ToList());
}

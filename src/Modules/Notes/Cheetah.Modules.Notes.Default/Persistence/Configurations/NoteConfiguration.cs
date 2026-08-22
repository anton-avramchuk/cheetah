using Cheetah.Modules.Notes.Default.Entities;
using Cheetah.Modules.Notes.Infrastructure.Persistence.Configurations;

namespace Cheetah.Modules.Notes.Default.Persistence.Configurations;

/// <summary>
/// EF-конфигурация заметки «из коробки». Доп. поля/индексы наследник добавляет, переопределив
/// <c>ConfigureCustom</c>.
/// </summary>
public sealed class NoteConfiguration : NoteConfigurationBase<Note>
{
}

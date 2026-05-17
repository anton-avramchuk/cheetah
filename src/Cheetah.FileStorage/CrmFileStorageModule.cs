using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.FileStorage;

/// <summary>
/// Core-модуль файлового хранилища. Сам по себе ничего не регистрирует — только определяет
/// абстракции. Подключите один из провайдеров: Cheetah.FileStorage.Local или
/// Cheetah.FileStorage.S3 (или несколько одновременно через keyed-services).
/// </summary>
[DependsOn(typeof(CoreModule))]
public partial class CrmFileStorageModule : CrmModule
{
}

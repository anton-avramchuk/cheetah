using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Saga.EntityFrameworkCore;

/// <summary>
/// EF Core реализация ISagaRepository. Регистрация выполняется явно через
/// services.AddEfSagaRepository&lt;TContext&gt;() в модуле-владельце DbContext'a.
/// DbContext должен реализовать ISagaDbContext и вызвать modelBuilder.AddSagas().
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmSagaModule))]
public partial class CrmSagaEntityFrameworkCoreModule : CrmModule
{
}

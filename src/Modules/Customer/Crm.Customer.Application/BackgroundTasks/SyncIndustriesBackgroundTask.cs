using Cheetah.BackgroundTasks;
using Cheetah.Core.DependencyInjection;
using Crm.Customer.Domain;
using Crm.MasterData.ApiClient;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crm.Customer.Application.BackgroundTasks;

[Export(LifetimeType.Singleton, typeof(IBackgroundTask))]
public class SyncIndustriesBackgroundTask(
    IServiceScopeFactory scopeFactory,
    ILogger<SyncIndustriesBackgroundTask> logger)
    : SyncBackgroundTask<IIndustryService, IndustryViewModel, CustomerIndustry, Guid>(scopeFactory, logger)
{
    public override TimeSpan Period => TimeSpan.FromMinutes(1);

    protected override async ValueTask<IReadOnlyList<IndustryViewModel>> FetchAsync(
        IIndustryService service, CancellationToken ct)
        => (await service.GetAllAsync(new GetIndustriesGridRequest { PageSize = 0 }, ct)).Data.ToList();

    protected override Guid GetId(IndustryViewModel vm) => vm.Id;

    protected override string ComputeItemHash(IndustryViewModel vm) => vm.ComputeHash();

    protected override string GetEntityContentHash(CustomerIndustry entity) => entity.ContentHash;

    protected override CustomerIndustry Create(IndustryViewModel vm) =>
        CustomerIndustry.Create(vm.Id, vm.Name);

    protected override void Update(CustomerIndustry entity, IndustryViewModel vm) =>
        entity.Update(vm.Name);
}

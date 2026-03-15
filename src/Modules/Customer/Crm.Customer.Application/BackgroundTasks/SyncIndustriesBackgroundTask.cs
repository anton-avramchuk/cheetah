using Cheetah.BackgroundTasks;
using Cheetah.Core.DataAccess.Abstractions;
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
    : PeriodicBackgroundTask
{
    public override TimeSpan Period => TimeSpan.FromMinutes(1);

    private volatile string? _lastBatchHash;

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var industryService = scope.ServiceProvider.GetRequiredService<IIndustryService>();

        var result = await industryService.GetAllAsync(
            new GetAllIndustriesRequest { PageSize = 0 }, cancellationToken);

        var industries = result.Data.ToList();

        if (industries.Count == 0)
        {
            logger.LogDebug("SyncIndustries: no industries returned from MasterData, skipping sync");
            return;
        }

        var batchHash = industries.ComputeBatchHash();
        if (batchHash == _lastBatchHash)
        {
            logger.LogDebug("SyncIndustries: no changes detected, skipping sync");
            return;
        }

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<CustomerIndustry, Guid>>();

        var existingById = (await repository.GetAllAsync(cancellationToken: cancellationToken))
            .ToDictionary(x => x.Id);

        var added = 0;
        var updated = 0;

        foreach (var industry in industries)
        {
            var hash = industry.ComputeHash();

            if (existingById.TryGetValue(industry.Id, out var local))
            {
                if (local.ContentHash == hash)
                    continue;

                local.Update(industry.Name, hash);
                repository.Update(local);
                updated++;
            }
            else
            {
                repository.Add(CustomerIndustry.Create(industry.Id, industry.Name, hash));
                added++;
            }
        }

        if (added > 0 || updated > 0)
            await repository.SaveChangesAsync(cancellationToken);

        _lastBatchHash = batchHash;

        if (added > 0 || updated > 0)
            logger.LogInformation(
                "SyncIndustries: synced industries from MasterData — added: {Added}, updated: {Updated}",
                added, updated);
    }
}

using System.Security.Cryptography;
using System.Text;
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
    private readonly Dictionary<Guid, string> _entityHashes = new();

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

        var batchHash = ComputeBatchHash(industries);
        if (batchHash == _lastBatchHash)
        {
            logger.LogDebug("SyncIndustries: no changes detected, skipping sync");
            return;
        }

        var changed = industries
            .Where(x => !_entityHashes.TryGetValue(x.Id, out var h) || h != ComputeEntityHash(x))
            .ToList();

        if (changed.Count == 0)
        {
            _lastBatchHash = batchHash;
            return;
        }

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<CustomerIndustry, Guid>>();

        var changedIds = changed.Select(x => x.Id).ToHashSet();
        var existingById = (await repository.GetAllAsync(cancellationToken: cancellationToken))
            .Where(x => changedIds.Contains(x.Id))
            .ToDictionary(x => x.Id);

        var added = 0;
        var updated = 0;

        foreach (var industry in changed)
        {
            if (existingById.TryGetValue(industry.Id, out var local))
            {
                local.Update(industry.Name);
                repository.Update(local);
                updated++;
            }
            else
            {
                repository.Add(CustomerIndustry.Create(industry.Id, industry.Name));
                added++;
            }
        }

        await repository.SaveChangesAsync(cancellationToken);

        foreach (var industry in changed)
            _entityHashes[industry.Id] = ComputeEntityHash(industry);

        _lastBatchHash = batchHash;

        logger.LogInformation(
            "SyncIndustries: synced industries from MasterData — added: {Added}, updated: {Updated}",
            added, updated);
    }

    private static string ComputeBatchHash(List<IndustryViewModel> industries)
    {
        var content = string.Join(";",
            industries.OrderBy(x => x.Id).Select(x => $"{x.Id}:{x.Name}"));
        return Hash(content);
    }

    private static string ComputeEntityHash(IndustryViewModel industry) =>
        Hash(industry.Name);

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}

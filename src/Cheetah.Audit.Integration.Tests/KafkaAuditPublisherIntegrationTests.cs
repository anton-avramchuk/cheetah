using Microsoft.EntityFrameworkCore;
using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Cheetah.Audit.Kafka;
using Cheetah.Backend.Events.Kafka;
using Cheetah.Core.Events;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Cheetah.Audit.Integration.Tests;

[Collection("Audit")]
public class KafkaAuditPublisherIntegrationTests
{
    private readonly AuditFixture _fx;

    public KafkaAuditPublisherIntegrationTests(AuditFixture fx) => _fx = fx;

    /// <summary>
    /// Поднимаем полную цепочку: KafkaAuditPublisher → keyed IEventBus(Kafka) → CrmKafkaEventBus
    /// → реальный Kafka. Consumer'ом подтверждаем что событие приехало.
    /// </summary>
    [Fact]
    public async Task End_To_End_AuditEntry_Reaches_Kafka_Via_Keyed_IEventBus()
    {
        var topicPrefix = $"audit-it-{Guid.NewGuid():N}-";

        await using (var seed = _fx.CreateDbContext(null))
        {
            await seed.Database.ExecuteSqlRawAsync("TRUNCATE \"AuditEntries\", \"Customers\"");
            seed.AuditEntries.Add(new AuditEntry
            {
                EntityType = "ItEntity",
                EntityId = "ent-1",
                Action = AuditAction.Created,
                Changes = "{\"k\":\"v\"}",
                OccurredAt = DateTimeOffset.UtcNow
            });
            await seed.SaveChangesAsync();
        }

        var sp = BuildServices(topicPrefix);
        var publisher = BuildPublisher(sp);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await publisher.StartAsync(cts.Token);

        var expectedTopic = $"{topicPrefix}{nameof(AuditEntryRecordedEvent)}";
        var received = await ConsumeOneAsync(_fx.KafkaBootstrapServers, expectedTopic, TimeSpan.FromSeconds(15));
        received.ShouldNotBeNull();
        received!.Value.ShouldContain("ItEntity");
        received.Value.ShouldContain("ent-1");

        await Task.Delay(500);
        await using (var verify = _fx.CreateDbContext(null))
        {
            var entry = verify.AuditEntries.Single(e => e.EntityId == "ent-1");
            entry.PublishedAt.ShouldNotBeNull();
        }

        await publisher.StopAsync(CancellationToken.None);
        sp.GetRequiredService<CrmKafkaEventBus>().Dispose();
    }

    [Fact]
    public async Task After_Publish_Pending_Queue_Is_Empty()
    {
        var topicPrefix = $"audit-drain-{Guid.NewGuid():N}-";

        await using (var seed = _fx.CreateDbContext(null))
        {
            await seed.Database.ExecuteSqlRawAsync("TRUNCATE \"AuditEntries\", \"Customers\"");
            for (var i = 0; i < 5; i++)
            {
                seed.AuditEntries.Add(new AuditEntry
                {
                    EntityType = "Drain", EntityId = $"id-{i}",
                    Action = AuditAction.Created, Changes = "{}",
                    OccurredAt = DateTimeOffset.UtcNow.AddSeconds(-i)
                });
            }
            await seed.SaveChangesAsync();
        }

        var sp = BuildServices(topicPrefix);
        var publisher = BuildPublisher(sp);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await publisher.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(5));
        await publisher.StopAsync(CancellationToken.None);
        sp.GetRequiredService<CrmKafkaEventBus>().Dispose();

        await using var verify = _fx.CreateDbContext(null);
        var unpublished = verify.AuditEntries.Count(e => e.EntityType == "Drain" && e.PublishedAt == null);
        unpublished.ShouldBe(0);
    }

    private ServiceProvider BuildServices(string topicPrefix)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_fx);
        services.AddScoped(sp => sp.GetRequiredService<AuditFixture>().CreateDbContext(null));
        services.AddScoped<IAuditPublishStore, EfAuditPublishStore<AuditDbContext>>();
        services.AddLogging();
        services.Configure<KafkaEventBusOptions>(o =>
        {
            o.BootstrapServers = _fx.KafkaBootstrapServers;
            o.TopicPrefix = topicPrefix;
            o.ClientId = "it-audit";
        });
        services.AddSingleton<IKafkaProducerFactory, DefaultKafkaProducerFactory>();
        services.AddSingleton<CrmKafkaEventBus>();
        services.AddKeyedSingleton<IEventBus>(EventBusKeys.Kafka,
            (sp, _) => sp.GetRequiredService<CrmKafkaEventBus>());
        return services.BuildServiceProvider();
    }

    private static KafkaAuditPublisher BuildPublisher(IServiceProvider sp)
        => new(
            sp,
            Microsoft.Extensions.Options.Options.Create(new KafkaAuditOptions
            {
                BatchSize = 10,
                PollingInterval = TimeSpan.FromMilliseconds(200)
            }),
            NullLogger<KafkaAuditPublisher>.Instance);

    private static async Task<ConsumeResult<string, string>?> ConsumeOneAsync(string bootstrap, string topic, TimeSpan timeout)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = $"test-{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (result?.Message is not null) return result;
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                // Topic ещё не создан producer'ом — продолжаем ждать.
                await Task.Delay(200);
            }
            await Task.Yield();
        }
        return null;
    }
}

using Microsoft.EntityFrameworkCore;
using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Cheetah.Audit.Kafka;
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

    [Fact]
    public async Task End_to_end_AuditEntry_попадает_в_Kafka_и_помечается_published()
    {
        var topic = $"audit-it-{Guid.NewGuid():N}";

        // 1. Создаём AuditEntry напрямую в БД
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

        // 2. Запускаем publisher
        var services = new ServiceCollection();
        services.AddSingleton(_fx);
        services.AddScoped(sp => sp.GetRequiredService<AuditFixture>().CreateDbContext(null));
        services.AddScoped<IAuditPublishStore, EfAuditPublishStore<AuditDbContext>>();
        var sp = services.BuildServiceProvider();

        var publisher = new KafkaAuditPublisher(
            sp,
            new DefaultKafkaProducerFactory(),
            Microsoft.Extensions.Options.Options.Create(new KafkaAuditOptions
            {
                BootstrapServers = _fx.KafkaBootstrapServers,
                Topic = topic,
                BatchSize = 10,
                PollingInterval = TimeSpan.FromMilliseconds(200)
            }),
            NullLogger<KafkaAuditPublisher>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await publisher.StartAsync(cts.Token);

        // 3. Consumer'ом ждём сообщения
        var received = await ConsumeOneAsync(_fx.KafkaBootstrapServers, topic, TimeSpan.FromSeconds(15));
        received.ShouldNotBeNull();
        received!.Value.ShouldContain("ItEntity");
        received.Value.ShouldContain("ent-1");
        received.Key.ShouldBe("ItEntity:ent-1");

        // 4. PublishedAt проставлен
        await Task.Delay(500); // дать publisher'у время на MarkPublished
        await using (var verify = _fx.CreateDbContext(null))
        {
            var entry = verify.AuditEntries.Single(e => e.EntityId == "ent-1");
            entry.PublishedAt.ShouldNotBeNull();
        }

        await publisher.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task После_публикации_pending_очередь_пустая()
    {
        var topic = $"audit-drain-{Guid.NewGuid():N}";

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

        var services = new ServiceCollection();
        services.AddSingleton(_fx);
        services.AddScoped(sp => sp.GetRequiredService<AuditFixture>().CreateDbContext(null));
        services.AddScoped<IAuditPublishStore, EfAuditPublishStore<AuditDbContext>>();

        var publisher = new KafkaAuditPublisher(
            services.BuildServiceProvider(),
            new DefaultKafkaProducerFactory(),
            Microsoft.Extensions.Options.Options.Create(new KafkaAuditOptions
            {
                BootstrapServers = _fx.KafkaBootstrapServers,
                Topic = topic,
                BatchSize = 10,
                PollingInterval = TimeSpan.FromMilliseconds(200)
            }),
            NullLogger<KafkaAuditPublisher>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await publisher.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(5));
        await publisher.StopAsync(CancellationToken.None);

        await using var verify = _fx.CreateDbContext(null);
        var unpublished = verify.AuditEntries.Count(e => e.EntityType == "Drain" && e.PublishedAt == null);
        unpublished.ShouldBe(0);
    }

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
            var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
            if (result?.Message is not null) return result;
            await Task.Yield();
        }
        return null;
    }
}

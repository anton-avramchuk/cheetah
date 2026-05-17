using Cheetah.Audit;
using Cheetah.Audit.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Cheetah.Audit.Tests;

public class KafkaAuditPublisherTests
{
    [Fact]
    public async Task Publisher_публикует_pending_и_помечает_published()
    {
        var pending = new[] { Sample("E1"), Sample("E2") };

        var store = new Mock<IAuditPublishStore>();
        var first = true;
        store.Setup(s => s.ClaimPendingAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(() => first
                ? new ValueTask<IReadOnlyList<AuditEntry>>(pending)
                : new ValueTask<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>()))
            .Callback(() => first = false);
        store.Setup(s => s.MarkPublishedAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var produced = new List<Message<string, string>>();
        var producer = new FakeProducer(m => { produced.Add(m); return new DeliveryResult<string, string>(); });
        var factory = new FakeFactory(producer);

        var sut = BuildPublisher(store.Object, factory);
        await RunOnce(sut);

        produced.Count.ShouldBe(2);
        produced[0].Value.ShouldContain("E1");
        store.Verify(s => s.MarkPublishedAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(pending[0].Id) && ids.Contains(pending[1].Id)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publisher_при_ProduceException_зовёт_MarkFailed()
    {
        var entry = Sample("FAIL");
        var store = new Mock<IAuditPublishStore>();
        var first = true;
        store.Setup(s => s.ClaimPendingAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(() => first
                ? new ValueTask<IReadOnlyList<AuditEntry>>(new[] { entry })
                : new ValueTask<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>()))
            .Callback(() => first = false);
        store.Setup(s => s.MarkFailedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var producer = new FakeProducer(_ => throw new ProduceException<string, string>(
            new Error(ErrorCode.NetworkException, "boom"),
            new DeliveryResult<string, string>()));
        var factory = new FakeFactory(producer);

        var sut = BuildPublisher(store.Object, factory);
        await RunOnce(sut);

        store.Verify(s => s.MarkFailedAsync(entry.Id,
            It.Is<string>(e => e.Contains("boom")),
            It.IsAny<DateTimeOffset>(),
            It.IsAny<CancellationToken>()), Times.Once);
        store.Verify(s => s.MarkPublishedAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Publisher_использует_EntityType_EntityId_как_Kafka_key()
    {
        var entry = Sample(entityType: "My.Entity", entityId: "id-42");
        var store = StoreOnce(entry);
        Message<string, string>? captured = null;
        var producer = new FakeProducer(m => { captured = m; return new DeliveryResult<string, string>(); });
        var factory = new FakeFactory(producer);

        await RunOnce(BuildPublisher(store, factory));

        captured.ShouldNotBeNull();
        captured!.Key.ShouldBe("My.Entity:id-42");
    }

    // ---------- helpers ----------

    private static AuditEntry Sample(string changesPayload = "{}", string entityId = "id-1", string entityType = "Sample.Entity") => new()
    {
        Id = Guid.NewGuid(),
        EntityType = entityType,
        EntityId = entityId,
        Action = AuditAction.Created,
        Changes = changesPayload,
        OccurredAt = DateTimeOffset.UtcNow
    };

    private static IAuditPublishStore StoreOnce(AuditEntry entry)
    {
        var m = new Mock<IAuditPublishStore>();
        var first = true;
        m.Setup(s => s.ClaimPendingAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(() => first
                ? new ValueTask<IReadOnlyList<AuditEntry>>(new[] { entry })
                : new ValueTask<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>()))
            .Callback(() => first = false);
        m.Setup(s => s.MarkPublishedAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        return m.Object;
    }

    private static KafkaAuditPublisher BuildPublisher(IAuditPublishStore store, IKafkaProducerFactory factory)
    {
        var services = new ServiceCollection();
        services.AddSingleton(store);
        var sp = services.BuildServiceProvider();

        return new KafkaAuditPublisher(
            sp, factory,
            Microsoft.Extensions.Options.Options.Create(new KafkaAuditOptions
            {
                Topic = "audit.test",
                PollingInterval = TimeSpan.FromMilliseconds(100)
            }),
            NullLogger<KafkaAuditPublisher>.Instance);
    }

    private static async Task RunOnce(KafkaAuditPublisher pub)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(800));
        await pub.StartAsync(cts.Token);
        await Task.Delay(400);
        await pub.StopAsync(CancellationToken.None);
    }

    private sealed class FakeFactory : IKafkaProducerFactory
    {
        private readonly IProducer<string, string> _producer;
        public FakeFactory(IProducer<string, string> producer) => _producer = producer;
        public IProducer<string, string> Create(KafkaAuditOptions options) => _producer;
    }

    private sealed class FakeProducer : IProducer<string, string>
    {
        private readonly Func<Message<string, string>, DeliveryResult<string, string>> _onProduce;
        public FakeProducer(Func<Message<string, string>, DeliveryResult<string, string>> onProduce) => _onProduce = onProduce;

        public Handle Handle => null!;
        public string Name => "fake";
        public int AddBrokers(string brokers) => 0;
        public void SetSaslCredentials(string username, string password) { }

        public Task<DeliveryResult<string, string>> ProduceAsync(string topic, Message<string, string> message, CancellationToken cancellationToken = default)
            => Task.FromResult(_onProduce(message));
        public Task<DeliveryResult<string, string>> ProduceAsync(TopicPartition topicPartition, Message<string, string> message, CancellationToken cancellationToken = default)
            => ProduceAsync(topicPartition.Topic, message, cancellationToken);
        public void Produce(string topic, Message<string, string> message, Action<DeliveryReport<string, string>>? deliveryHandler = null) => throw new NotImplementedException();
        public void Produce(TopicPartition topicPartition, Message<string, string> message, Action<DeliveryReport<string, string>>? deliveryHandler = null) => throw new NotImplementedException();
        public int Poll(TimeSpan timeout) => 0;
        public int Flush(TimeSpan timeout) => 0;
        public void Flush(CancellationToken cancellationToken = default) { }
        public void InitTransactions(TimeSpan timeout) { }
        public void BeginTransaction() { }
        public void CommitTransaction(TimeSpan timeout) { }
        public void CommitTransaction() { }
        public void AbortTransaction(TimeSpan timeout) { }
        public void AbortTransaction() { }
        public void SendOffsetsToTransaction(IEnumerable<TopicPartitionOffset> offsets, IConsumerGroupMetadata groupMetadata, TimeSpan timeout) { }
        public void Dispose() { }
    }
}

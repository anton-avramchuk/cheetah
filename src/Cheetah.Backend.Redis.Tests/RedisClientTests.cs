using Cheetah.Backend.Redis;
using Shouldly;
using Moq;
using StackExchange.Redis;
using System.Text.Json;

namespace Cheetah.Backend.Redis.Tests;

public class RedisClientTests
{
    private readonly Mock<IRedisConnectionProvider> _connectionProviderMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly RedisClient _client;

    public RedisClientTests()
    {
        _connectionProviderMock = new Mock<IRedisConnectionProvider>();
        _databaseMock = new Mock<IDatabase>();

        _connectionProviderMock
            .Setup(x => x.GetDatabase(It.IsAny<string>()))
            .Returns(_databaseMock.Object);

        _client = new RedisClient(_connectionProviderMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnDeserializedValue()
    {
        // Arrange
        var testData = new TestData { Id = 1, Name = "Test" };
        var serialized = JsonSerializer.Serialize(testData);
        _databaseMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)serialized);

        // Act
        var result = await _client.GetAsync<TestData>("test-key");

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(1);
        result.Name.ShouldBe("Test");
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _databaseMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _client.GetAsync<TestData>("nonexistent-key");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldSerializeAndStoreValue()
    {
        // Arrange
        var testData = new TestData { Id = 1, Name = "Test" };
        _databaseMock
            .Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _client.SetAsync("test-key", testData);

        // Assert
        result.ShouldBeTrue();
        _databaseMock.Verify(x => x.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.Is<RedisValue>(v => v.ToString().Contains("Test")),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithExpiry_ShouldPassExpiryToRedis()
    {
        // Arrange
        var testData = new TestData { Id = 1, Name = "Test" };
        var expiry = TimeSpan.FromMinutes(5);
        _databaseMock
            .Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _client.SetAsync("test-key", testData, expiry);

        // Assert
        result.ShouldBeTrue();
        _databaseMock.Verify(x => x.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            expiry,
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallKeyDelete()
    {
        // Arrange
        _databaseMock
            .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _client.DeleteAsync("test-key");

        // Assert
        result.ShouldBeTrue();
        _databaseMock.Verify(x => x.KeyDeleteAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        // Arrange
        _databaseMock
            .Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _client.ExistsAsync("test-key");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _databaseMock
            .Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        var result = await _client.ExistsAsync("test-key");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnMultipleValues()
    {
        // Arrange
        var testData1 = new TestData { Id = 1, Name = "Test1" };
        var testData2 = new TestData { Id = 2, Name = "Test2" };
        var values = new[]
        {
            (RedisValue)JsonSerializer.Serialize(testData1),
            (RedisValue)JsonSerializer.Serialize(testData2)
        };

        _databaseMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(values);

        // Act
        var result = await _client.GetManyAsync<TestData>(new[] { "key1", "key2" });

        // Assert
        result.Count.ShouldBe(2);
        result["key1"]!.Name.ShouldBe("Test1");
        result["key2"]!.Name.ShouldBe("Test2");
    }

    [Fact]
    public async Task SetManyAsync_ShouldSetMultipleValues()
    {
        // Arrange
        var values = new Dictionary<string, TestData>
        {
            ["key1"] = new TestData { Id = 1, Name = "Test1" },
            ["key2"] = new TestData { Id = 2, Name = "Test2" }
        };

        var batchMock = new Mock<IBatch>();
        batchMock
            .Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock
            .Setup(x => x.CreateBatch(It.IsAny<object>()))
            .Returns(batchMock.Object);

        // Act
        var result = await _client.SetManyAsync(values);

        // Assert
        result.ShouldBeTrue();
        batchMock.Verify(x => x.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAsync_WithCustomInstanceName_ShouldUseCorrectInstance()
    {
        // Arrange
        var testData = new TestData { Id = 1, Name = "Test" };
        var serialized = JsonSerializer.Serialize(testData);
        _databaseMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)serialized);

        // Act
        await _client.GetAsync<TestData>("test-key", "cache");

        // Assert
        _connectionProviderMock.Verify(x => x.GetDatabase("cache"), Times.Once);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

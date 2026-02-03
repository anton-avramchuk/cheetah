using Cheetah.Backend.Redis;
using Shouldly;
using Microsoft.Extensions.Options;

namespace Cheetah.Backend.Redis.Tests;

public class RedisConnectionProviderTests : IDisposable
{
    private readonly RedisOptions _options;

    public RedisConnectionProviderTests()
    {
        _options = new RedisOptions
        {
            Instances = new Dictionary<string, RedisInstanceOptions>
            {
                ["default"] = new RedisInstanceOptions
                {
                    ConnectionString = "localhost:6379,abortConnect=false",
                    Database = 0
                },
                ["cache"] = new RedisInstanceOptions
                {
                    ConnectionString = "localhost:6379,abortConnect=false",
                    Database = 1
                }
            }
        };
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void GetConnection_WithDefaultInstance_ShouldReturnConnection()
    {
        // Arrange
        var optionsMock = Options.Create(_options);
        using var provider = new RedisConnectionProvider(optionsMock);

        // Act
        var act = () => provider.GetConnection("default");

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void GetConnection_WithNamedInstance_ShouldReturnConnection()
    {
        // Arrange
        var optionsMock = Options.Create(_options);
        using var provider = new RedisConnectionProvider(optionsMock);

        // Act
        var act = () => provider.GetConnection("cache");

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    public void GetConnection_WithNonExistentInstance_ShouldThrowException()
    {
        // Arrange
        var optionsMock = Options.Create(_options);
        using var provider = new RedisConnectionProvider(optionsMock);

        // Act
        var act = () => provider.GetConnection("nonexistent");

        // Assert
        Should.Throw<InvalidOperationException>(act).Message.ShouldContain("nonexistent");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void GetDatabase_WithDefaultInstance_ShouldReturnDatabase()
    {
        // Arrange
        var optionsMock = Options.Create(_options);
        using var provider = new RedisConnectionProvider(optionsMock);

        // Act
        var act = () => provider.GetDatabase("default");

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void GetDatabase_WithNamedInstance_ShouldReturnDatabase()
    {
        // Arrange
        var optionsMock = Options.Create(_options);
        using var provider = new RedisConnectionProvider(optionsMock);

        // Act
        var act = () => provider.GetDatabase("cache");

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    public void GetDatabase_WithNonExistentInstance_ShouldThrowException()
    {
        // Arrange
        var optionsMock = Options.Create(_options);
        using var provider = new RedisConnectionProvider(optionsMock);

        // Act
        var act = () => provider.GetDatabase("nonexistent");

        // Assert
        Should.Throw<InvalidOperationException>(act);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void GetConnection_CalledMultipleTimes_ShouldReturnSameInstance()
    {
        // Arrange
        var optionsMock = Options.Create(_options);
        using var provider = new RedisConnectionProvider(optionsMock);

        // Act
        var connection1 = provider.GetConnection("default");
        var connection2 = provider.GetConnection("default");

        // Assert
        connection1.ShouldBeSameAs(connection2);
    }

    public void Dispose()
    {
    }
}

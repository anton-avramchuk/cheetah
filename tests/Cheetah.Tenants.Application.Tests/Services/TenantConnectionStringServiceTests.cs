using Cheetah.Core.Tenants.Services;
using Cheetah.Tenants.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Cheetah.Tenants.Application.Tests.Services;

public class TenantConnectionStringServiceTests
{
    [Fact]
    public async Task GenerateAllConnectionStringsAsync_ShouldGenerateForAllProviders()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantName = "Acme Corporation";
        var baseConnectionString = "Host=localhost;Database=template";

        var identityProviderMock = new Mock<IModuleConnectionStringProvider>();
        identityProviderMock.Setup(x => x.ModuleName).Returns("Identity");
        identityProviderMock.Setup(x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString))
            .Returns("Host=localhost;Database=acme_identity");

        var providers = new List<IModuleConnectionStringProvider> { identityProviderMock.Object };

        var inMemorySettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:TenantTemplate", baseConnectionString}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var service = new TenantConnectionStringService(providers, configuration);

        // Act
        var result = await service.GenerateAllConnectionStringsAsync(tenantId, tenantName);

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey("Identity");
        result["Identity"].Should().Be("Host=localhost;Database=acme_identity");
    }

    [Fact]
    public async Task GenerateAllConnectionStringsAsync_WithNoProviders_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantName = "Test Tenant";
        var baseConnectionString = "Host=localhost;Database=template";

        var providers = new List<IModuleConnectionStringProvider>();

        var inMemorySettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:TenantTemplate", baseConnectionString}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var service = new TenantConnectionStringService(providers, configuration);

        // Act
        var result = await service.GenerateAllConnectionStringsAsync(tenantId, tenantName);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateAllConnectionStringsAsync_WithoutTenantTemplate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantName = "Test Tenant";

        var providerMock = new Mock<IModuleConnectionStringProvider>();
        providerMock.Setup(x => x.ModuleName).Returns("Identity");

        var providers = new List<IModuleConnectionStringProvider> { providerMock.Object };

        var inMemorySettings = new Dictionary<string, string>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var service = new TenantConnectionStringService(providers, configuration);

        // Act
        Func<Task> act = async () => await service.GenerateAllConnectionStringsAsync(tenantId, tenantName);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*TenantTemplate*not configured*");
    }

    [Fact]
    public async Task GenerateAllConnectionStringsAsync_WithMultipleProviders_ShouldGenerateAll()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantName = "Acme Corporation";
        var baseConnectionString = "Host=localhost;Database=template";

        var identityProviderMock = new Mock<IModuleConnectionStringProvider>();
        identityProviderMock.Setup(x => x.ModuleName).Returns("Identity");
        identityProviderMock.Setup(x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString))
            .Returns("Host=localhost;Database=acme_identity");

        var featuresProviderMock = new Mock<IModuleConnectionStringProvider>();
        featuresProviderMock.Setup(x => x.ModuleName).Returns("Features");
        featuresProviderMock.Setup(x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString))
            .Returns("Host=localhost;Database=acme_features");

        var permissionsProviderMock = new Mock<IModuleConnectionStringProvider>();
        permissionsProviderMock.Setup(x => x.ModuleName).Returns("Permissions");
        permissionsProviderMock.Setup(x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString))
            .Returns("Host=localhost;Database=acme_permissions");

        var providers = new List<IModuleConnectionStringProvider>
        {
            identityProviderMock.Object,
            featuresProviderMock.Object,
            permissionsProviderMock.Object
        };

        var inMemorySettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:TenantTemplate", baseConnectionString}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var service = new TenantConnectionStringService(providers, configuration);

        // Act
        var result = await service.GenerateAllConnectionStringsAsync(tenantId, tenantName);

        // Assert
        result.Should().HaveCount(3);
        result.Should().ContainKey("Identity");
        result.Should().ContainKey("Features");
        result.Should().ContainKey("Permissions");
        result["Identity"].Should().Be("Host=localhost;Database=acme_identity");
        result["Features"].Should().Be("Host=localhost;Database=acme_features");
        result["Permissions"].Should().Be("Host=localhost;Database=acme_permissions");

        // Verify each provider was called exactly once
        identityProviderMock.Verify(x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString), Times.Once);
        featuresProviderMock.Verify(x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString), Times.Once);
        permissionsProviderMock.Verify(x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString), Times.Once);
    }

    [Fact]
    public async Task GenerateAllConnectionStringsAsync_WithProviders_ShouldCallEachProviderOnce()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantName = "Test Corp";
        var baseConnectionString = "Host=localhost;Database=template";

        var identityProviderMock = new Mock<IModuleConnectionStringProvider>();
        identityProviderMock.Setup(x => x.ModuleName).Returns("Identity");
        identityProviderMock.Setup(x => x.GenerateConnectionString(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("Host=localhost;Database=test_identity");

        var featuresProviderMock = new Mock<IModuleConnectionStringProvider>();
        featuresProviderMock.Setup(x => x.ModuleName).Returns("Features");
        featuresProviderMock.Setup(x => x.GenerateConnectionString(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("Host=localhost;Database=test_features");

        var providers = new List<IModuleConnectionStringProvider>
        {
            identityProviderMock.Object,
            featuresProviderMock.Object
        };

        var inMemorySettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:TenantTemplate", baseConnectionString}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var service = new TenantConnectionStringService(providers, configuration);

        // Act
        await service.GenerateAllConnectionStringsAsync(tenantId, tenantName);

        // Assert - Verify correct parameters were passed
        identityProviderMock.Verify(
            x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString),
            Times.Once);
        featuresProviderMock.Verify(
            x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAllConnectionStringsAsync_WithDuplicateModuleNames_ShouldUseLastProvider()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantName = "Test Tenant";
        var baseConnectionString = "Host=localhost;Database=template";

        var firstProviderMock = new Mock<IModuleConnectionStringProvider>();
        firstProviderMock.Setup(x => x.ModuleName).Returns("Identity");
        firstProviderMock.Setup(x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString))
            .Returns("Host=localhost;Database=first_identity");

        var secondProviderMock = new Mock<IModuleConnectionStringProvider>();
        secondProviderMock.Setup(x => x.ModuleName).Returns("Identity");
        secondProviderMock.Setup(x => x.GenerateConnectionString(tenantId, tenantName, baseConnectionString))
            .Returns("Host=localhost;Database=second_identity");

        var providers = new List<IModuleConnectionStringProvider>
        {
            firstProviderMock.Object,
            secondProviderMock.Object
        };

        var inMemorySettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:TenantTemplate", baseConnectionString}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var service = new TenantConnectionStringService(providers, configuration);

        // Act
        var result = await service.GenerateAllConnectionStringsAsync(tenantId, tenantName);

        // Assert - Dictionary should contain only one entry with the last provider's value
        result.Should().HaveCount(1);
        result.Should().ContainKey("Identity");
        result["Identity"].Should().Be("Host=localhost;Database=second_identity");
    }
}

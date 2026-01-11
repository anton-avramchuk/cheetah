using Cheetah.Core.Tenants.Services;
using FluentAssertions;

namespace Cheetah.Core.Tenants.Tests.Services;

public class DefaultModuleConnectionStringProviderTests
{
    [Fact]
    public void Constructor_ShouldSetModuleName()
    {
        // Arrange
        var moduleName = "Identity";

        // Act
        var provider = new DefaultModuleConnectionStringProvider(moduleName);

        // Assert
        provider.ModuleName.Should().Be(moduleName);
    }

    [Fact]
    public void GenerateConnectionString_WithPostgreSql_ShouldGenerateCorrectConnectionString()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("Identity");
        var tenantId = Guid.NewGuid();
        var tenantName = "AcmeCorp";
        var baseConnectionString = "Host=localhost;Port=5432;Database=template;Username=postgres;Password=secret";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.Should().Contain("tenant_AcmeCorp_identity");
        result.ToLowerInvariant().Should().Contain("host=localhost");
        result.Should().Contain("5432");
        result.ToLowerInvariant().Should().Contain("username=postgres");
        result.ToLowerInvariant().Should().Contain("password=secret");
    }

    [Fact]
    public void GenerateConnectionString_WithSqlServer_ShouldGenerateCorrectConnectionString()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("Features");
        var tenantId = Guid.NewGuid();
        var tenantName = "TestCompany";
        var baseConnectionString = "Server=localhost;Database=template;User Id=sa;Password=Test123;";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.Should().Contain("tenant_TestCompany_features");
        result.ToLowerInvariant().Should().Contain("server=localhost");
        result.ToLowerInvariant().Should().Contain("user id=sa");
        result.ToLowerInvariant().Should().Contain("password=test123");
    }

    [Fact]
    public void GenerateConnectionString_WithMySql_ShouldGenerateCorrectConnectionString()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("Permissions");
        var tenantId = Guid.NewGuid();
        var tenantName = "MyOrg";
        var baseConnectionString = "Server=localhost;Database=template;Uid=root;Pwd=password;";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.Should().Contain("tenant_MyOrg_permissions");
        result.ToLowerInvariant().Should().Contain("server=localhost");
        result.ToLowerInvariant().Should().Contain("uid=root");
        result.ToLowerInvariant().Should().Contain("pwd=password");
    }

    [Fact]
    public void GenerateConnectionString_ShouldConvertModuleNameToLowerCase()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("IDENTITY");
        var tenantId = Guid.NewGuid();
        var tenantName = "TestTenant";
        var baseConnectionString = "Host=localhost;Database=template";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.Should().Contain("tenant_TestTenant_identity");
        result.Should().NotContain("IDENTITY");
    }

    [Fact]
    public void GenerateConnectionString_ShouldUseTenantName()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("Identity");
        var tenantId = Guid.NewGuid();
        var tenantName = "UniqueCompany";
        var baseConnectionString = "Host=localhost;Database=template";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.Should().Contain("tenant_UniqueCompany_identity");
    }

    [Fact]
    public void GenerateConnectionString_WithDifferentTenants_ShouldGenerateDifferentDatabases()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("Identity");
        var tenantId1 = Guid.NewGuid();
        var tenantId2 = Guid.NewGuid();
        var tenantName1 = "Tenant1";
        var tenantName2 = "Tenant2";
        var baseConnectionString = "Host=localhost;Database=template";

        // Act
        var result1 = provider.GenerateConnectionString(tenantId1, tenantName1, baseConnectionString);
        var result2 = provider.GenerateConnectionString(tenantId2, tenantName2, baseConnectionString);

        // Assert
        result1.Should().Contain("tenant_Tenant1_identity");
        result2.Should().Contain("tenant_Tenant2_identity");
        result1.Should().NotBe(result2);
    }

    [Fact]
    public void GenerateConnectionString_WithSpacesInTenantName_ShouldIncludeSpaces()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("Identity");
        var tenantId = Guid.NewGuid();
        var tenantName = "Acme Corporation";
        var baseConnectionString = "Host=localhost;Database=template";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.Should().Contain("tenant_Acme Corporation_identity");
    }

    [Fact]
    public void GenerateConnectionString_WithUnderscoresInTenantName_ShouldPreserveUnderscores()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("Features");
        var tenantId = Guid.NewGuid();
        var tenantName = "test_company_123";
        var baseConnectionString = "Host=localhost;Database=template";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.Should().Contain("tenant_test_company_123_features");
    }

    [Fact]
    public void GenerateConnectionString_WithDifferentModules_ShouldGenerateDifferentDatabases()
    {
        // Arrange
        var identityProvider = new DefaultModuleConnectionStringProvider("Identity");
        var featuresProvider = new DefaultModuleConnectionStringProvider("Features");
        var tenantId = Guid.NewGuid();
        var tenantName = "SameTenant";
        var baseConnectionString = "Host=localhost;Database=template";

        // Act
        var identityResult = identityProvider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);
        var featuresResult = featuresProvider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        identityResult.Should().Contain("tenant_SameTenant_identity");
        featuresResult.Should().Contain("tenant_SameTenant_features");
        identityResult.Should().NotBe(featuresResult);
    }

    [Fact]
    public void GenerateConnectionString_ShouldPreserveOtherConnectionStringProperties()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("Identity");
        var tenantId = Guid.NewGuid();
        var tenantName = "Test";
        var baseConnectionString = "Host=db.example.com;Port=5432;Database=template;Username=user;Password=pass;Pooling=true;Timeout=30";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.ToLowerInvariant().Should().Contain("host=db.example.com");
        result.Should().Contain("5432");
        result.ToLowerInvariant().Should().Contain("username=user");
        result.ToLowerInvariant().Should().Contain("password=pass");
        result.ToLowerInvariant().Should().Contain("pooling=true");
        result.Should().Contain("30");
        result.Should().Contain("tenant_Test_identity");
    }

    [Fact]
    public void GenerateConnectionString_WithEmptyModuleName_ShouldGenerateWithoutModuleSuffix()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("");
        var tenantId = Guid.NewGuid();
        var tenantName = "TestTenant";
        var baseConnectionString = "Host=localhost;Database=template";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.Should().Contain("tenant_TestTenant_");
    }

    [Fact]
    public void GenerateConnectionString_WithMixedCaseModuleName_ShouldConvertToLowerCase()
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider("MyModuleName");
        var tenantId = Guid.NewGuid();
        var tenantName = "Tenant";
        var baseConnectionString = "Host=localhost;Database=template";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.Should().Contain("tenant_Tenant_mymodulename");
    }

    [Theory]
    [InlineData("Identity", "Acme", "tenant_Acme_identity")]
    [InlineData("Features", "TestCo", "tenant_TestCo_features")]
    [InlineData("Permissions", "MyOrg", "tenant_MyOrg_permissions")]
    [InlineData("UPPERCASE", "Company", "tenant_Company_uppercase")]
    public void GenerateConnectionString_WithVariousInputs_ShouldGenerateExpectedDatabaseName(
        string moduleName,
        string tenantName,
        string expectedDbName)
    {
        // Arrange
        var provider = new DefaultModuleConnectionStringProvider(moduleName);
        var tenantId = Guid.NewGuid();
        var baseConnectionString = "Host=localhost;Database=template";

        // Act
        var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

        // Assert
        result.Should().Contain(expectedDbName);
    }
}

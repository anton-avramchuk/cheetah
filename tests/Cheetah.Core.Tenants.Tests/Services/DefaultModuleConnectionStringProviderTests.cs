using Cheetah.Core.Tenants.Services;
using Shouldly;

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
        provider.ModuleName.ShouldBe(moduleName);
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
        result.ShouldContain("tenant_AcmeCorp_identity");
        result.ToLowerInvariant().ShouldContain("host=localhost");
        result.ShouldContain("5432");
        result.ToLowerInvariant().ShouldContain("username=postgres");
        result.ToLowerInvariant().ShouldContain("password=secret");
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
        result.ShouldContain("tenant_TestCompany_features");
        result.ToLowerInvariant().ShouldContain("server=localhost");
        result.ToLowerInvariant().ShouldContain("user id=sa");
        result.ToLowerInvariant().ShouldContain("password=test123");
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
        result.ShouldContain("tenant_MyOrg_permissions");
        result.ToLowerInvariant().ShouldContain("server=localhost");
        result.ToLowerInvariant().ShouldContain("uid=root");
        result.ToLowerInvariant().ShouldContain("pwd=password");
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
        result.ShouldContain("tenant_TestTenant_identity");
        result.ShouldNotContain("IDENTITY", Case.Sensitive);
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
        result.ShouldContain("tenant_UniqueCompany_identity");
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
        result1.ShouldContain("tenant_Tenant1_identity");
        result2.ShouldContain("tenant_Tenant2_identity");
        result1.ShouldNotBe(result2);
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
        result.ShouldContain("tenant_Acme Corporation_identity");
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
        result.ShouldContain("tenant_test_company_123_features");
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
        identityResult.ShouldContain("tenant_SameTenant_identity");
        featuresResult.ShouldContain("tenant_SameTenant_features");
        identityResult.ShouldNotBe(featuresResult);
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
        result.ToLowerInvariant().ShouldContain("host=db.example.com");
        result.ShouldContain("5432");
        result.ToLowerInvariant().ShouldContain("username=user");
        result.ToLowerInvariant().ShouldContain("password=pass");
        result.ToLowerInvariant().ShouldContain("pooling=true");
        result.ShouldContain("30");
        result.ShouldContain("tenant_Test_identity");
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
        result.ShouldContain("tenant_TestTenant_");
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
        result.ShouldContain("tenant_Tenant_mymodulename");
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
        result.ShouldContain(expectedDbName);
    }
}

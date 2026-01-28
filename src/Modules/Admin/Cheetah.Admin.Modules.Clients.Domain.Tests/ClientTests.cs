using FluentAssertions;

namespace Cheetah.Admin.Modules.Clients.Domain.Tests;

public class ClientTests
{
    #region Create

    [Fact]
    public void Create_WithValidName_ShouldCreateClient()
    {
        // Arrange
        var name = "Test Client";

        // Act
        var client = Client.Create(name);

        // Assert
        client.Should().NotBeNull();
        client.Id.Should().NotBeEmpty();
        client.Name.Should().Be(name);
        client.Description.Should().BeNull();
        client.TenantId.Should().BeNull();
    }

    [Fact]
    public void Create_WithNameAndDescription_ShouldCreateClient()
    {
        // Arrange
        var name = "Test Client";
        var description = "Test Description";

        // Act
        var client = Client.Create(name, description);

        // Assert
        client.Name.Should().Be(name);
        client.Description.Should().Be(description);
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateClient()
    {
        // Arrange
        var name = "Test Client";
        var description = "Test Description";
        var tenantId = Guid.NewGuid();

        // Act
        var client = Client.Create(name, description, tenantId);

        // Assert
        client.Name.Should().Be(name);
        client.Description.Should().Be(description);
        client.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act
        var client1 = Client.Create("Client 1");
        var client2 = Client.Create("Client 2");

        // Assert
        client1.Id.Should().NotBe(client2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => Client.Create(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Update

    [Fact]
    public void Update_WithValidName_ShouldUpdateClient()
    {
        // Arrange
        var client = Client.Create("Original Name", "Original Description");

        // Act
        client.Update("Updated Name", "Updated Description");

        // Assert
        client.Name.Should().Be("Updated Name");
        client.Description.Should().Be("Updated Description");
    }

    [Fact]
    public void Update_WithNullDescription_ShouldClearDescription()
    {
        // Arrange
        var client = Client.Create("Original Name", "Original Description");

        // Act
        client.Update("Updated Name", null);

        // Assert
        client.Name.Should().Be("Updated Name");
        client.Description.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldNotChangeId()
    {
        // Arrange
        var client = Client.Create("Original Name");
        var originalId = client.Id;

        // Act
        client.Update("Updated Name");

        // Assert
        client.Id.Should().Be(originalId);
    }

    [Fact]
    public void Update_ShouldNotChangeTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var client = Client.Create("Original Name", null, tenantId);

        // Act
        client.Update("Updated Name");

        // Assert
        client.TenantId.Should().Be(tenantId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var client = Client.Create("Original Name");

        // Act
        var act = () => client.Update(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region AssignToTenant

    [Fact]
    public void AssignToTenant_ShouldSetTenantId()
    {
        // Arrange
        var client = Client.Create("Test Client");
        var tenantId = Guid.NewGuid();

        // Act
        client.AssignToTenant(tenantId);

        // Assert
        client.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void AssignToTenant_ShouldOverwriteExistingTenantId()
    {
        // Arrange
        var originalTenantId = Guid.NewGuid();
        var newTenantId = Guid.NewGuid();
        var client = Client.Create("Test Client", null, originalTenantId);

        // Act
        client.AssignToTenant(newTenantId);

        // Assert
        client.TenantId.Should().Be(newTenantId);
    }

    #endregion

    #region AggregateRoot

    [Fact]
    public void Client_ShouldHaveEmptyDomainEventsInitially()
    {
        // Act
        var client = Client.Create("Test Client");

        // Assert
        client.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ShouldClearAllEvents()
    {
        // Arrange
        var client = Client.Create("Test Client");

        // Act
        client.ClearDomainEvents();

        // Assert
        client.DomainEvents.Should().BeEmpty();
    }

    #endregion
}

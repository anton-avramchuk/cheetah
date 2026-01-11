using Cheetah.Features.Domain.Entities;
using Cheetah.Features.Events;
using FluentAssertions;

namespace Cheetah.Features.Domain.Tests;

public class FeatureTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateFeature()
    {
        // Arrange
        var id = "advanced-reporting";
        var displayName = "Advanced Reporting";
        var description = "Enable advanced reporting features";
        var group = "Reporting";

        // Act
        var feature = Feature.Create(id, displayName, description, isEnabledByDefault: true, group);

        // Assert
        feature.Should().NotBeNull();
        feature.Id.Should().Be(id);
        feature.Name.Should().Be(id);
        feature.DisplayName.Should().Be(displayName);
        feature.Description.Should().Be(description);
        feature.IsEnabledByDefault.Should().BeTrue();
        feature.Group.Should().Be(group);
        feature.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        feature.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithMinimalData_ShouldCreateFeature()
    {
        // Arrange
        var id = "feature-1";
        var displayName = "Feature 1";

        // Act
        var feature = Feature.Create(id, displayName);

        // Assert
        feature.Id.Should().Be(id);
        feature.DisplayName.Should().Be(displayName);
        feature.Description.Should().BeNull();
        feature.IsEnabledByDefault.Should().BeFalse();
        feature.Group.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseFeatureCreatedEvent()
    {
        // Arrange
        var id = "feature-1";
        var displayName = "Feature 1";

        // Act
        var feature = Feature.Create(id, displayName);

        // Assert
        feature.DomainEvents.Should().HaveCount(1);
        var domainEvent = feature.DomainEvents.First();
        domainEvent.Should().BeOfType<FeatureCreatedEvent>();
        var createdEvent = domainEvent as FeatureCreatedEvent;
        createdEvent!.FeatureId.Should().Be(id);
        createdEvent.Name.Should().Be(id);
        createdEvent.DisplayName.Should().Be(displayName);
        createdEvent.IsEnabledByDefault.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyId_ShouldThrowArgumentException(string? invalidId)
    {
        // Act
        Action act = () => Feature.Create(invalidId!, "Display Name");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Feature ID cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDisplayName_ShouldThrowArgumentException(string? invalidDisplayName)
    {
        // Act
        Action act = () => Feature.Create("feature-id", invalidDisplayName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Display name cannot be empty*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateFeature()
    {
        // Arrange
        var feature = Feature.Create("feature-1", "Original Name");
        var newDisplayName = "Updated Name";
        var newDescription = "Updated description";
        var newGroup = "New Group";

        // Act
        feature.Update(newDisplayName, newDescription, newGroup);

        // Assert
        feature.DisplayName.Should().Be(newDisplayName);
        feature.Description.Should().Be(newDescription);
        feature.Group.Should().Be(newGroup);
        feature.UpdatedAt.Should().NotBeNull();
        feature.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_WithMinimalData_ShouldUpdateOnlyDisplayName()
    {
        // Arrange
        var feature = Feature.Create("feature-1", "Original Name", "Original description", group: "Original Group");
        var newDisplayName = "Updated Name";

        // Act
        feature.Update(newDisplayName);

        // Assert
        feature.DisplayName.Should().Be(newDisplayName);
        feature.Description.Should().BeNull();
        feature.Group.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_WithEmptyDisplayName_ShouldThrowArgumentException(string? invalidDisplayName)
    {
        // Arrange
        var feature = Feature.Create("feature-1", "Original Name");

        // Act
        Action act = () => feature.Update(invalidDisplayName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Display name cannot be empty*");
    }

    [Fact]
    public void Update_ShouldSetUpdatedAt()
    {
        // Arrange
        var feature = Feature.Create("feature-1", "Original Name");
        var beforeUpdate = DateTime.UtcNow;

        // Act
        feature.Update("Updated Name");

        // Assert
        feature.UpdatedAt.Should().NotBeNull();
        feature.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void SetDefaultEnabled_WithTrue_ShouldEnableByDefault()
    {
        // Arrange
        var feature = Feature.Create("feature-1", "Feature 1", isEnabledByDefault: false);

        // Act
        feature.SetDefaultEnabled(true);

        // Assert
        feature.IsEnabledByDefault.Should().BeTrue();
        feature.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetDefaultEnabled_WithFalse_ShouldDisableByDefault()
    {
        // Arrange
        var feature = Feature.Create("feature-1", "Feature 1", isEnabledByDefault: true);

        // Act
        feature.SetDefaultEnabled(false);

        // Assert
        feature.IsEnabledByDefault.Should().BeFalse();
        feature.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetDefaultEnabled_ShouldSetUpdatedAt()
    {
        // Arrange
        var feature = Feature.Create("feature-1", "Feature 1");
        var beforeUpdate = DateTime.UtcNow;

        // Act
        feature.SetDefaultEnabled(true);

        // Assert
        feature.UpdatedAt.Should().NotBeNull();
        feature.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void Feature_WithComplexScenario_ShouldMaintainCorrectState()
    {
        // Arrange
        var id = "multi-factor-auth";
        var feature = Feature.Create(id, "Multi-Factor Authentication");

        // Act - Multiple updates
        feature.Update("MFA", "Two-factor authentication", "Security");
        feature.SetDefaultEnabled(true);
        feature.Update("Multi-Factor Auth", "Enhanced security with 2FA", "Security");

        // Assert
        feature.Id.Should().Be(id);
        feature.DisplayName.Should().Be("Multi-Factor Auth");
        feature.Description.Should().Be("Enhanced security with 2FA");
        feature.Group.Should().Be("Security");
        feature.IsEnabledByDefault.Should().BeTrue();
        feature.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Feature_IdShouldBeImmutable()
    {
        // Arrange
        var originalId = "immutable-feature";
        var feature = Feature.Create(originalId, "Test Feature");

        // Act
        feature.Update("Updated Name", "Updated description", "Updated group");

        // Assert
        feature.Id.Should().Be(originalId); // Id should not change
    }

    [Fact]
    public void Feature_NameShouldMatchId()
    {
        // Arrange
        var id = "feature-name-test";

        // Act
        var feature = Feature.Create(id, "Display Name");

        // Assert
        feature.Name.Should().Be(id);
        feature.Name.Should().Be(feature.Id);
    }
}

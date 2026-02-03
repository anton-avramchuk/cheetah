using Shouldly;

namespace Cheetah.Admin.Modules.Clients.Domain.Tests;

public class TariffTests
{
    #region Create

    [Fact]
    public void Create_WithValidParameters_ShouldCreateTariff()
    {
        // Arrange
        var name = "Basic Plan";
        var price = 9.99m;
        var currency = "USD";

        // Act
        var tariff = Tariff.Create(name, price, currency);

        // Assert
        tariff.ShouldNotBeNull();
        tariff.Id.ShouldNotBe(Guid.Empty);
        tariff.Name.ShouldBe(name);
        tariff.Price.ShouldBe(price);
        tariff.Currency.ShouldBe("USD");
        tariff.Description.ShouldBeNull();
        tariff.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateTariff()
    {
        // Arrange
        var name = "Premium Plan";
        var price = 29.99m;
        var currency = "eur";
        var description = "Premium features included";
        var isActive = false;

        // Act
        var tariff = Tariff.Create(name, price, currency, description, isActive);

        // Assert
        tariff.Name.ShouldBe(name);
        tariff.Price.ShouldBe(price);
        tariff.Currency.ShouldBe("EUR"); // Should be uppercase
        tariff.Description.ShouldBe(description);
        tariff.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act
        var tariff1 = Tariff.Create("Plan 1", 10m, "USD");
        var tariff2 = Tariff.Create("Plan 2", 20m, "EUR");

        // Assert
        tariff1.Id.ShouldNotBe(tariff2.Id);
    }

    [Fact]
    public void Create_ShouldConvertCurrencyToUppercase()
    {
        // Act
        var tariff = Tariff.Create("Plan", 10m, "usd");

        // Assert
        tariff.Currency.ShouldBe("USD");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => Tariff.Create(name!, 10m, "USD");

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCurrency_ShouldThrowArgumentException(string? currency)
    {
        // Act
        var act = () => Tariff.Create("Plan", 10m, currency!);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("A")]
    public void Create_WithInvalidCurrencyLength_ShouldThrowArgumentException(string currency)
    {
        // Act
        var act = () => Tariff.Create("Plan", 10m, currency);

        // Assert
        Should.Throw<ArgumentException>(act).Message.ShouldContain("3-character");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () => Tariff.Create("Plan", -1m, "USD");

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldSucceed()
    {
        // Act
        var tariff = Tariff.Create("Free Plan", 0m, "USD");

        // Assert
        tariff.Price.ShouldBe(0m);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateTariff()
    {
        // Arrange
        var tariff = Tariff.Create("Original", 10m, "USD", "Original desc", true);

        // Act
        tariff.Update("Updated", 20m, "EUR", "Updated desc", false);

        // Assert
        tariff.Name.ShouldBe("Updated");
        tariff.Price.ShouldBe(20m);
        tariff.Currency.ShouldBe("EUR");
        tariff.Description.ShouldBe("Updated desc");
        tariff.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Update_ShouldNotChangeId()
    {
        // Arrange
        var tariff = Tariff.Create("Original", 10m, "USD");
        var originalId = tariff.Id;

        // Act
        tariff.Update("Updated", 20m, "EUR", null, true);

        // Assert
        tariff.Id.ShouldBe(originalId);
    }

    [Fact]
    public void Update_WithNullDescription_ShouldClearDescription()
    {
        // Arrange
        var tariff = Tariff.Create("Plan", 10m, "USD", "Description");

        // Act
        tariff.Update("Plan", 10m, "USD", null, true);

        // Assert
        tariff.Description.ShouldBeNull();
    }

    [Fact]
    public void Update_ShouldConvertCurrencyToUppercase()
    {
        // Arrange
        var tariff = Tariff.Create("Plan", 10m, "USD");

        // Act
        tariff.Update("Plan", 10m, "eur", null, true);

        // Assert
        tariff.Currency.ShouldBe("EUR");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var tariff = Tariff.Create("Original", 10m, "USD");

        // Act
        var act = () => tariff.Update(name!, 10m, "USD", null, true);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Update_WithInvalidCurrencyLength_ShouldThrowArgumentException(string currency)
    {
        // Arrange
        var tariff = Tariff.Create("Plan", 10m, "USD");

        // Act
        var act = () => tariff.Update("Plan", 10m, currency, null, true);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Update_WithNegativePrice_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var tariff = Tariff.Create("Plan", 10m, "USD");

        // Act
        var act = () => tariff.Update("Plan", -1m, "USD", null, true);

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    #endregion

    #region AggregateRoot

    [Fact]
    public void Tariff_ShouldHaveEmptyDomainEventsInitially()
    {
        // Act
        var tariff = Tariff.Create("Plan", 10m, "USD");

        // Assert
        tariff.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ShouldClearAllEvents()
    {
        // Arrange
        var tariff = Tariff.Create("Plan", 10m, "USD");

        // Act
        tariff.ClearDomainEvents();

        // Assert
        tariff.DomainEvents.ShouldBeEmpty();
    }

    #endregion
}

using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class AmountTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(1050)]
    [InlineData(int.MaxValue)]
    public void AcceptsPositiveAmountsInMinorUnits(int minorUnits)
    {
        // Act
        var amount = new Amount(minorUnits, "GBP");

        // Assert
        Assert.Equal(minorUnits, amount.MinorUnits);
        Assert.Equal("GBP", amount.Currency.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsZeroOrNegativeAmounts(int minorUnits)
    {
        // Act
        var exception = Assert.Throws<InvalidPaymentException>(() => new Amount(minorUnits, "GBP"));

        // Assert
        Assert.Equal("amount", exception.Field);
    }

    [Fact]
    public void ReportsTheCurrencyFirstWhenCurrencyAndAmountAreInvalid()
    {
        // Act
        var exception = Assert.Throws<InvalidPaymentException>(() => new Amount(0, "JPY"));

        // Assert
        Assert.Equal("currency", exception.Field);
    }
}
